using UnityEngine;
using Photon.Pun;

public class CharacterController
{
    protected GameObject _character;
    protected Rigidbody _rb;
    public bool IsMine => _character.GetPhotonView().IsMine;

    protected float _moveSpeed = 4;
    

    public void SetCharacterObject(GameObject character)
    {
        _character = character;
        _rb = _character.GetComponent<Rigidbody>();
    }

    public virtual void Move(Vector2 input) //공용 이동 로직
    {
        Vector3 dir = new Vector3(input.x, 0f, input.y);
        Vector3 velocity = dir.normalized * _moveSpeed;

        _rb.linearVelocity = new Vector3(velocity.x, 0f, velocity.z);
    }

    public virtual void Look(Vector2 direction) //공용 시점 이동 로직
    {
        Vector3 dir = new Vector3(direction.x, 0f, direction.y);
        Quaternion rot = Quaternion.LookRotation(dir);

        _rb.MoveRotation(rot);
    }

    public virtual void Vault() //공용 창틀 뛰어넘기 로직
    {

    }
}
