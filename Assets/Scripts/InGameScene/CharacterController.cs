using UnityEngine;

public class CharacterController : MonoBehaviour
{
    protected GameObject _character;
    protected Rigidbody _rb;

    protected float _moveSpeed;
    private void Awake()
    {
        _rb = _character.GetComponent<Rigidbody>();
    }

    public void SetCharacterObject(GameObject character)
    {
        _character = character;
    }

    public virtual void Move(Vector2 input)
    {
        Vector3 dir = new Vector3(input.x, 0f, input.y);
        Vector3 velocity = dir.normalized * _moveSpeed;

        _rb.linearVelocity = new Vector3(velocity.x, 0f, velocity.z);
    }

    public virtual void Look(Vector2 direction)
    {
        Vector3 dir = new Vector3(direction.x, 0f, direction.y);
        Quaternion rot = Quaternion.LookRotation(dir);

        _rb.MoveRotation(rot);
    }
}
