using UnityEngine;

public class KillerInputActionResolver
{
    private KillerController _killerController;    

    public KillerInputActionResolver(KillerController controller)
    {
        _killerController = controller;
    }

    public ICommand ResolveMove(Vector2 input)
    {        
        return new MoveCommand(_killerController, input);
    }

    
}
