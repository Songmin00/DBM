

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

public class InteractCommand : ICommand //수리, 치료, 구출 통합
{
    SurvivorController _survivorController;
    public InteractCommand(SurvivorController controller)
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
