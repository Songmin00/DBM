using Photon.Pun;
using UnityEngine;

public class SurvivorController : CharacterControllerBase, IPunObservable
{
    [SerializeField] Transform _cameraAnchor;
    private SurvivorStateManager _stateManager;
    private SurvivorInteractManager _interactManager;
    private Animator _animator;
    private CapsuleCollider _capsuleCollider;
    private PhotonView _pv;
    public PhotonView Pv => _pv;
    private float _currentAnimSpeed = 0f;

    protected override void Awake()
    {
        base.Awake();
        _pv = GetComponent<PhotonView>();
        _stateManager = GetComponent<SurvivorStateManager>();
        _interactManager = GetComponent<SurvivorInteractManager>();
        _animator = GetComponent<Animator>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        if (_cameraAnchor == null) _cameraAnchor = transform.GetChild(0);
    }
    private void Update()
    {
        // 들려있는 상태(CanMove가 false인 상황 등)라면 부모의 위치에 강제 고정
        if (_stateManager != null && !_stateManager.CanMove() && transform.parent != null)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
    protected override void FixedUpdate()
    {
        if (_pv == null || !_pv.IsMine) return;
        base.FixedUpdate();
        if (_stateManager != null && !_stateManager.CanMove()) return;
        MoveRogic();
    }

    public void SetMoveSpeed(float speed) => MoveSpeed = speed;

    public void SetPhysical(bool enable)
    {
        if (_rb != null)
        {
            _rb.isKinematic = !enable;
            _rb.useGravity = enable;

            if (!enable) // 들렸을 때
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                // 중요: 보간을 끄지 않으면 이전 위치로 돌아가려는 관성이 남습니다.
                _rb.interpolation = RigidbodyInterpolation.None;
            }
            else // 내려놓을 때
            {
                _rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        // PhotonTransformView가 켜져 있으면 네트워크 위치 보정이 들어와서 행성 현상이 생깁니다.
        var ptv = GetComponent<PhotonTransformView>();
        if (ptv != null) ptv.enabled = enable;

        if (_capsuleCollider != null) _capsuleCollider.enabled = enable;
    }

    public void Run(bool running)
    {
        if (!_pv.IsMine || (_stateManager != null && (!_stateManager.CanMove() || _stateManager.IsSitting))) return;
        SetMoveSpeed(running ? 6f : 4f);
    }

    public void Sit(bool sit)
    {
        if (!_pv.IsMine) return;
        if (_stateManager != null) _stateManager.SetSitting(sit);
        SetMoveSpeed(sit ? 2f : 4f);
    }

    public void Interact(bool interact)
    {
        if (!_pv.IsMine || _interactManager == null) return;
        if (interact)
        {
            if (_interactManager.Nearest == null) return;
            _interactManager.StartInteract();
            LookAtTarget(_interactManager.Nearest.transform);
            if (_stateManager != null) _stateManager.SetInteracting(true);
        }
        else
        {
            _interactManager.StopInteract();
            if (_stateManager != null) _stateManager.SetInteracting(false);
        }
    }

    private void LookAtTarget(Transform target)
    {
        if (target == null) return;
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude <= 0.0001f) return;
        _rb.MoveRotation(Quaternion.LookRotation(dir, Vector3.up));
    }

    [PunRPC] public void RPC_Hit() => _stateManager?.OnHit();
   
    protected override void MoveRogic()
    {
        if (!_pv.IsMine) return;
        bool isMoving = MoveInput.sqrMagnitude > 0.0001f;
        _currentAnimSpeed = isMoving ? MoveSpeed : 0f;
        if (_animator != null) _animator.SetFloat("Speed", _currentAnimSpeed);

        if (!isMoving)
        {
            _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
            return;
        }

        Transform cam = Camera.main.transform;
        Vector3 dir = (cam.forward * MoveInput.y + cam.right * MoveInput.x);
        dir.y = 0;
        dir.Normalize();

        _rb.linearVelocity = new Vector3(dir.x * MoveSpeed, _rb.linearVelocity.y, dir.z * MoveSpeed);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, Quaternion.LookRotation(dir), 15f * Time.fixedDeltaTime));
    }

    public Transform GetCameraAnchor() => _cameraAnchor;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) stream.SendNext(_currentAnimSpeed);
        else
        {
            _currentAnimSpeed = (float)stream.ReceiveNext();
            if (_animator != null) _animator.SetFloat("Speed", _currentAnimSpeed);
        }
    }
}