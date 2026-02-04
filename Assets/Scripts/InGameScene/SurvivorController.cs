using Photon.Pun;
using UnityEngine;

public class SurvivorController : CharacterControllerBase, IPunObservable
{
    [Header("이동속도 스탯")]
    public float WalkSpeed = 2.26f;
    public float RunSpeed = 4f;
    public float SitSpeed = 1.13f;
    public float DyingSpeed = 0.7f;

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

            if (!enable)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.interpolation = RigidbodyInterpolation.None;
                _rb.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                _rb.interpolation = RigidbodyInterpolation.Interpolate;
                _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
        }
        
        var ptv = GetComponent<PhotonTransformView>();
        if (ptv != null)
        {
            ptv.enabled = enable;
        }
        if (_capsuleCollider != null)
        {
            _capsuleCollider.enabled = enable;
        }
    }

    public void Run(bool running)
    {
        if (!_pv.IsMine || (_stateManager != null && (!_stateManager.CanMove() || _stateManager.IsSitting))) return;
        if (_stateManager.CurrentState != SurvivorState.None && _stateManager.CurrentState != SurvivorState.Hurt) return;
        SetMoveSpeed(running ? RunSpeed : WalkSpeed);
    }

    public void Sit(bool sit)
    {
        if (!_pv.IsMine) return;
        if (_stateManager != null) _stateManager.SetSitting(sit);
        SetMoveSpeed(sit ? SitSpeed : WalkSpeed);
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

    [PunRPC]
    public void RPC_Hit()
    {
        _stateManager?.OnHit();
    }
    
   
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