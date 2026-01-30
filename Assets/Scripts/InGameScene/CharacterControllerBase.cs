using UnityEngine;
using Photon.Pun;

public class CharacterControllerBase : MonoBehaviour //개별 캐릭터 프리팹에 부착
{    
    protected Rigidbody _rb;
    public bool IsMine => gameObject.GetPhotonView().IsMine;

    protected float _moveSpeed = 4; //캐릭터 스탯 구현 후 받아오기

    public Vector2 MoveInput {  get; private set; }

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    protected virtual void FixedUpdate()
    {
        if (!IsMine)
        {
            return;
        }
        MoveRogic();
    }


    public virtual void Move(Vector2 input) //공용 이동 로직
    {        
        MoveInput = input;
    }


    public virtual void Vault() //공용 창틀 뛰어넘기 로직
    {

    }

    protected virtual void MoveRogic()
    {
        Vector3 currentVelocity = _rb.linearVelocity; // 정지시 중력 정상 반영을 위한 관성 값 저장

        if (MoveInput == Vector2.zero) //입력 없으면 즉시 정지
        {
            _rb.linearVelocity = new Vector3(0, currentVelocity.y, 0);
            return;
        }
        Vector3 dir = new Vector3(MoveInput.x, 0f, MoveInput.y);

        Transform cam = Camera.main.transform;

        Vector3 camForward = cam.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cam.right;
        camRight.y = 0f;
        camRight.Normalize();

        dir = camForward * MoveInput.y + camRight * MoveInput.x;

        Vector3 velocity = dir.normalized * _moveSpeed;

        _rb.linearVelocity = new Vector3(velocity.x, 0f, velocity.z);

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        _rb.MoveRotation(targetRot);
    }
}
