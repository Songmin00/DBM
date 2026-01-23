

public class RunCommand : ICommand //달리기
{
    SurvivorController _survivorController;

    public RunCommand(SurvivorController controller)
    {
        _survivorController = controller;
    }

    public void Execute()
    {
        
    }
}

public class SitCommand : ICommand //앉기
{
    SurvivorController _survivorController;
    public SitCommand(SurvivorController controller)
    {
        _survivorController = controller;
    }

    public void Execute()
    {

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