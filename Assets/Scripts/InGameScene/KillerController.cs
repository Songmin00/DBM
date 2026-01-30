using UnityEngine;
using UnityEngine.Windows;

//킬러가 가지고 있는 공통 액션만 정의. 각각 특수능력은 KillerAbility로.
public class KillerController : CharacterControllerBase
{    
    [SerializeField] Transform _cameraAnchor;    
    [SerializeField] Transform _pitchPivot;    
    [SerializeField] Transform _cameraTarget;

    [SerializeField] float _yawSpeed = 20f;
    [SerializeField] float _pitchSpeed = 10f;
    [SerializeField] float _minPitch = -30f;
    [SerializeField] float _maxPitch = 20f;
    Vector2 _lookDirection;
    float _currentPitch;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        LookRogic();
    }

    public Transform GetCameraAnchor()
    {
        return _cameraAnchor;
    }
    public Transform GetCameraTarget()
    {
        return _cameraTarget;
    }

    public void Look(Vector2 input) //몸통 회전
    {
        _lookDirection = input;
    }


    public void Attack() //단거리 공격
    {

    }

    public void Lunge() //대쉬 공격
    {

    }

    public void KickPanel() //판자 부수기
    {

    }

    public void KickGenerator() //발전기 부수기
    {

    }

    public void Lift() //쓰러진 생존자 들기
    {

    }

    public void Drop() //들고 있는 생존자 내려놓기
    {

    }

    protected override void MoveRogic()
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
    }

    private void LookRogic()
    {
        //좌우회전은 몸통 통째로 돌리기
        float yaw = _lookDirection.x * _yawSpeed * Time.fixedDeltaTime; 
        Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);
        _rb.MoveRotation(_rb.rotation * yawRot);

        // 상하 회전은 카메라 피치 피봇만 이동하기
        _currentPitch -= _lookDirection.y * _pitchSpeed * Time.fixedDeltaTime;
        _currentPitch = Mathf.Clamp(_currentPitch, _minPitch, _maxPitch);

        _pitchPivot.localRotation = Quaternion.Euler(_currentPitch, 0f, 0f);
    }
}
