using UnityEngine;

public class SurvivorInputActionResolver
{
    private SurvivorController _survivorController;

    public SurvivorInputActionResolver(SurvivorController controller)
    {
        _survivorController = controller;
    }

    public ICommand ResolveMove(Vector2 input)
    {
        return new MoveCommand(_survivorController, input);
    }

    public ICommand ResoleveLook(Vector2 input)
    {
        return new NullCommand();
    }

    public ICommand ResolveInteract(bool interact)
    {
        return new SurvivorInteractCommand(_survivorController, interact);
    }

    public ICommand ResolveSit(bool sit)
    {
        return new SitCommand(_survivorController, sit);
    }
}
