
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

public class KickPanelAttackCommand : ICommand //판자 부수기
{
    KillerController _controller;

    public KickPanelAttackCommand(KillerController characterController)
    {
        _controller = characterController;
    }

    public void Execute()
    {
        
    }
}

public class KickGeneratorAttackCommand : ICommand //발전기 부수기
{
    KillerController _controller;

    public KickGeneratorAttackCommand(KillerController characterController)
    {
        _controller = characterController;
    }

    public void Execute()
    {

    }
}      
       
public class LiftAttackCommand : ICommand //쓰러진 생존자 들기
{
    KillerController _controller;

    public LiftAttackCommand(KillerController characterController)
    {
        _controller = characterController;
    }

    public void Execute()
    {

    }
}      
       
public class DropAttackCommand : ICommand //들고 있는 생존자 내려놓기
{
    KillerController _controller;

    public DropAttackCommand(KillerController characterController)
    {
        _controller = characterController;
    }

    public void Execute()
    {

    }
}
