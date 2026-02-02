using UnityEngine;
using Photon.Pun;

public class CharacterControllerBase : MonoBehaviour
{
    protected Rigidbody _rb;
    public bool IsMine => gameObject.GetPhotonView().IsMine;

    public float MoveSpeed { get; set; } = 4;
    public Vector2 MoveInput { get; private set; }

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        // 물리 회전 차단 (X, Z)
        _rb.constraints = RigidbodyConstraints.FreezeRotationX
                        | RigidbodyConstraints.FreezeRotationZ;

        // 마찰로 인한 미세 회전 방지
        _rb.angularDamping = 0f;
    }

    protected virtual void FixedUpdate()
    {
        if (!IsMine) return;

        // ★ 핵심: 외부 충돌로 인한 뱅글뱅글 회전 강제 차단
        _rb.angularVelocity = Vector3.zero;
    }

    public virtual void Move(Vector2 input)
    {
        MoveInput = input;
    }

    public virtual void Vault() { }

    protected virtual void MoveRogic()
    {
        if (!IsMine) return;

        if (MoveInput == Vector2.zero)
        {
            _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
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

        _rb.linearVelocity = new Vector3(velocity.x, _rb.linearVelocity.y, velocity.z);

        // 생존자용: 이동 방향으로 즉시 회전
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            _rb.MoveRotation(targetRot);
        }
    }
}