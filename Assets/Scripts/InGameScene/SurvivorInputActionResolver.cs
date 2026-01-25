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
}
