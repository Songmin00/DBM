using UnityEngine;
using Photon.Pun;

public class CharacterControllerBase : MonoBehaviour //개별 캐릭터 프리팹에 부착
{    
    protected Rigidbody _rb;
    public bool IsMine => gameObject.GetPhotonView().IsMine;

    protected float _moveSpeed = 4; //캐릭터 스탯 구현 후 받아오기

    public Vector2 MoveInput {  get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 currentVelocity = _rb.linearVelocity;

        if (MoveInput == Vector2.zero) //입력 없으면 즉시 정지
        {
            _rb.linearVelocity = new Vector3(0, currentVelocity.y, 0);
            return;
        }
        Vector3 dir = new Vector3(MoveInput.x, 0f, MoveInput.y);
        Vector3 velocity = dir.normalized * _moveSpeed;

        _rb.linearVelocity = new Vector3(velocity.x, 0f, velocity.z);
    }


    public virtual void Move(Vector2 input) //공용 이동 로직
    {        
        MoveInput = input;
    }

    public virtual void Look(Vector2 direction) //공용 시점 이동 로직
    {
        if (direction == Vector2.zero)
        {
            return;
        }
        Vector3 dir = new Vector3(direction.x, 0f, direction.y);
        Quaternion rot = Quaternion.LookRotation(dir);

        _rb.MoveRotation(rot);
    }

    public virtual void Vault() //공용 창틀 뛰어넘기 로직
    {

    }
}
