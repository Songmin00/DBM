using UnityEngine;

//모든 커맨드는 이걸 상속
public interface ICommand //커맨드는 한 프레임짜리 행동을 정의하는 단위로 사용.
{
    void Execute();
}

public class NullCommand : ICommand
{
    public void Execute()
    {

    }
}

//공용 이동 & 방향전환 커맨드 정의
public class MoveCommand : ICommand
{
    CharacterControllerBase _controller;
    Vector2 _input;

    public MoveCommand(CharacterControllerBase controller, Vector2 input)
    {
        _controller = controller;
        _input = input;
    }

    public void Execute()
    {
        _controller.Move(_input);
    }
}

public class LookCommand : ICommand
{
    KillerController _controller;
    Vector2 _input;

    public LookCommand(KillerController controller, Vector2 input)
    {
        _controller = controller;
        _input = input;
    }

    public void Execute()
    {
        _controller.Look(_input);
    }
}

public class VaultCommand : ICommand
{
    CharacterControllerBase _controller;

    public VaultCommand(CharacterControllerBase controller)
    {
        _controller = controller;
    }

    public void Execute()
    {
        _controller.Vault();
    }
}

public class AttackCommand : ICommand //단거리 공격
{
    KillerController _controller;

    public AttackCommand(KillerController characterController)
    {
        _controller = characterController;
    }

    public void Execute()
    {
        _controller.Attack();
    }
}

public class LungeAttackCommand : ICommand //대쉬 공격
{
    KillerController _controller;

    public LungeAttackCommand(KillerController characterController)
    {
        _controller = characterController;
    }

    public void Execute()
    {

    }
}

public class KickPanelCommand : ICommand //판자 부수기
{
    KillerController _controller;

    public KickPanelCommand(KillerController characterController)
    {
        _controller = characterController;
    }

    public void Execute()
    {

    }
}

public class KickGeneratorCommand : ICommand //발전기 부수기
{
    KillerController _controller;

    public KickGeneratorCommand(KillerController characterController)
    {
        _controller = characterController;
    }

    public void Execute()
    {

    }
}

public class KillerInteractCommand : ICommand //쓰러진 생존자 들기
{
    KillerController _controller;

    public KillerInteractCommand(KillerController characterController)
    {
        _controller = characterController;
    }

    public void Execute()
    {
        _controller.Interact();
    }
}

public class DropCommand : ICommand //들고 있는 생존자 내려놓기
{
    KillerController _controller;

    public DropCommand(KillerController characterController)
    {
        _controller = characterController;
    }

    public void Execute()
    {

    }
}

public class CatchCommand : ICommand
{
    KillerController _controller;

    public CatchCommand(KillerController characterController)
    {
        _controller = characterController;
    }

    public void Execute()
    {

    }
}

public class RunCommand : ICommand //달리기
{
    SurvivorController _survivorController;
    bool _isRun;
    public RunCommand(SurvivorController controller, bool isHold)
    {
        _survivorController = controller;
        _isRun = isHold;
    }

    public void Execute()
    {
        _survivorController.Run(_isRun);
    }
}

public class SitCommand : ICommand //앉기
{
    SurvivorController _survivorController;
    bool _isSit;

    public SitCommand(SurvivorController controller, bool isSit)
    {
        _survivorController = controller;
        _isSit = isSit;
    }

    public void Execute()
    {
        _survivorController.Sit(_isSit);
    }
}

public class DownPanelCommand : ICommand //판자 내리기
{
    SurvivorController _survivorController;
    public DownPanelCommand(SurvivorController controller)
    {
        _survivorController = controller;
    }

    public void Execute()
    {

    }
}

public class FixGeneratorCommand : ICommand //수리
{
    SurvivorController _survivorController;
    public FixGeneratorCommand(SurvivorController controller)
    {
        _survivorController = controller;
    }

    public void Execute()
    {

    }
}

public class HealCommand : ICommand //치료
{
    SurvivorController _survivorController;
    public HealCommand(SurvivorController controller)
    {
        _survivorController = controller;
    }

    public void Execute()
    {

    }
}

public class ResquerCommand : ICommand //구출
{
    SurvivorController _survivorController;
    public ResquerCommand(SurvivorController controller)
    {
        _survivorController = controller;
    }

    public void Execute()
    {

    }
}

public class SkillCheckCommand : ICommand //미니게임 스킬 체크
{
    SurvivorController _survivorController;
    public SkillCheckCommand(SurvivorController controller)
    {
        _survivorController = controller;
    }

    public void Execute()
    {

    }
}

public class UseItemCommand : ICommand //아이템 사용
{
    SurvivorController _survivorController;
    public UseItemCommand(SurvivorController controller)
    {
        _survivorController = controller;
    }

    public void Execute()
    {

    }
}

public class SurvivorInteractCommand : ICommand
{
    SurvivorController _survivorController;
    bool _interact;

    public SurvivorInteractCommand(SurvivorController controller, bool interact)
    {
        _survivorController = controller;
        _interact = interact;
    }

    public void Execute()
    {
        _survivorController.Interact(_interact);
    }
}
