using UnityEngine;

//모든 커맨드는 이걸 상속
public interface ICommand //커맨드는 한 프레임짜리 행동을 정의하는 단위로 사용.
{
    void Execute();
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
    CharacterControllerBase _controller;
    Vector2 _input;

    public LookCommand(CharacterControllerBase controller, Vector2 input)
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

