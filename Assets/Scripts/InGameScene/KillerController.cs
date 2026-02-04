using Photon.Pun;
using UnityEngine;

public class KillerController : CharacterControllerBase, IPunObservable
{
    [Header("카메라 위치(좌우 회전축)")]
    [SerializeField] Transform _cameraAnchor;
    [Header("CameraAnchor의 자식(상하 회전 축)")]
    [SerializeField] Transform _pitchPivot;
    [Header("PitchPivot의 자식(카메라가 바라볼 지점)")]
    [SerializeField] Transform _cameraTarget;
    [SerializeField] Transform _liftPoint;

    [Header("좌우 회전 속도")]
    [SerializeField] float _yawSpeed = 50;
    [Header("상하 회전 속도")]
    [SerializeField] float _pitchSpeed = 30f;
    [SerializeField] float _minPitch = -30f;
    [SerializeField] float _maxPitch = 20f;

    KillerInteractManager _interactManager;
    Vector2 _lookDirection;
    float _currentPitch;
    Animator _animator;
    bool _isAttacking;
    bool _isDelaying;

    protected override void Awake()
    {
        base.Awake();
        _interactManager = GetComponent<KillerInteractManager>();
        _animator = GetComponent<Animator>();
        MoveSpeed = 4.6f;
    }

    private void Update()
    {
        if (!IsMine) return;

        // 마우스 회전은 Update에서 처리
        LookUpdate();
    }

    protected override void FixedUpdate()
    {
        if (!IsMine) return;
        base.FixedUpdate(); // 외부회전 막기
        MoveRogic();
    }

    public Transform GetLiftPoint() => _liftPoint;
    public Transform GetCameraAnchor() => _cameraAnchor;
    public Transform GetCameraTarget() => _cameraTarget;

    public void Look(Vector2 input)
    {
        _lookDirection = input;
    }

    public void LookAtTarget(Transform target)
    {
        if (target == null) return;
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        _rb.MoveRotation(targetRot);
    }

    public void Attack()
    {
        if (!IsMine || _isAttacking) return;
        SoundManager.Instance.PlaySFX("AttackSound", 0.6f);
        _isAttacking = true;
        _animator.SetBool("IsAttack", true);
    }

    public void OnAttackHit()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        _isDelaying = true;
        Vector3 center = transform.position + transform.forward * 1.8f;
        Vector3 halfExtents = new Vector3(0.7f, 1.0f, 1.0f);

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation, LayerMask.GetMask("Survivor"));
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out SurvivorController survivor))
                survivor.Pv.RPC("RPC_Hit", RpcTarget.All);
        }
    }

    public void OnAttackEnd()
    {
        _isAttacking = _isDelaying = false;
        _animator.SetBool("IsAttack", false);
    }

    public void Interact() => _interactManager.StartInteract();

    protected override void MoveRogic()
    {
        if (!IsMine || _isDelaying) return;

        if (MoveInput == Vector2.zero)
        {
            _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
            _animator.SetFloat("Speed", 0f);
            return;
        }

        Transform cam = Camera.main.transform;
        Vector3 camForward = cam.forward;
        camForward.y = 0f;
        camForward.Normalize();
        Vector3 camRight = cam.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 dir = camForward * MoveInput.y + camRight * MoveInput.x;
        Vector3 velocity = dir.normalized * MoveSpeed;

        _animator.SetFloat("Speed", MoveSpeed);
        _rb.linearVelocity = new Vector3(velocity.x, _rb.linearVelocity.y, velocity.z);
    }

    private void LookUpdate()
    {
        if (_isDelaying) return;

        float yaw = _lookDirection.x * _yawSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, yaw);

        _currentPitch -= _lookDirection.y * _pitchSpeed * Time.deltaTime;
        _currentPitch = Mathf.Clamp(_currentPitch, _minPitch, _maxPitch);
        _pitchPivot.localRotation = Quaternion.Euler(_currentPitch, 0f, 0f);

        _lookDirection = Vector2.zero;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) stream.SendNext(MoveSpeed);
        else MoveSpeed = (float)stream.ReceiveNext();
    }
}