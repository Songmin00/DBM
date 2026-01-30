using UnityEngine;

//생존자 공용 액션을 정의. 경우에 따라 발전기 수리와 판자 내리기 등은 인터페이스를 통해 Interact()로 통합시킬 것.
public class SurvivorController : CharacterControllerBase
{
    [SerializeField] Transform _cameraAnchor;
    [SerializeField] SurvivorInteractManager _interactManager;
    bool _isInteracting = false;

    protected override void Awake()
    {
        base.Awake();

        if (_cameraAnchor == null)
        {
            _cameraAnchor = gameObject.transform.GetChild(0);
        }
    }

    protected override void FixedUpdate()
    {
        if (_isInteracting)
        {
            return;
        }
        MoveRogic();
    }

    public Transform GetCameraAnchor()
    {
        return _cameraAnchor;
    }

    public void Run() //달리기
    {

    }
    
    public void Sit() //앉기
    {

    }

    public void DownPanel() //판자 내리기
    {

    }

    public void Interact(bool interact)
    {
        if (interact)
        {
            _interactManager.StartInteract();
            _isInteracting = true;
            Debug.Log("인터랙션 시작!");
        }
        else
        {
            _interactManager.StopInteract();
            _isInteracting = false;
            Debug.Log("인터랙션 종료!");
        }        
    }    

    public void SkillCheck() //미니게임 스킬 체크
    {

    }

    public void FixGenerator() //발전기 수리
    {

    }


    public void Heal() //생존자 치료
    {

    }

    public void Resque() //생존자 구출
    {

    }

    public void UseItem()
    {

    }

    protected override void MoveRogic()
    {
        
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

        Vector3 dir = (camForward * MoveInput.y + camRight * MoveInput.x).normalized;
        Vector3 velocity = dir * _moveSpeed;

        
        _rb.linearVelocity = new Vector3(velocity.x, _rb.linearVelocity.y, velocity.z);

        
        if (dir != Vector3.zero)
        {        
            Quaternion targetRot = Quaternion.LookRotation(dir);
            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, 15f * Time.fixedDeltaTime));
        }
    }
}
