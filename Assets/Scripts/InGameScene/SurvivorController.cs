using UnityEngine;

//생존자 공용 액션을 정의. 경우에 따라 발전기 수리와 판자 내리기 등은 인터페이스를 통해 Interact()로 통합시킬 것.
public class SurvivorController : CharacterControllerBase
{
    [SerializeField] Transform _cameraAnchor;

    protected override void Awake()
    {
        base.Awake();

        if (_cameraAnchor == null)
        {
            _cameraAnchor = gameObject.transform.GetChild(0);
        }
    }

    public Transform GetCameraAnchor()
    {
        return _cameraAnchor;
    }

    public override void Move(Vector2 input) //공용 이동 로직
    {
        base.Move(input);
        Look(input);
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

}
