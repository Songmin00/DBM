
public class AttackCommand : ICommand //단거리 공격
{
    KillerController _controller;

    public AttackCommand(KillerController characterController)
    {
        _controller = characterController;
    }

    public void Execute()
    {
        
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
       
public class LiftCommand : ICommand //쓰러진 생존자 들기
{
    KillerController _controller;

    public LiftCommand(KillerController characterController)
    {
        _controller = characterController;
    }

    public void Execute()
    {

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
