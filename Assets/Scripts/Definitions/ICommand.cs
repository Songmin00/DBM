using UnityEngine;

//모든 커맨드는 이걸 상속
public interface ICommand
{
    void Execute();
}


//공용 이동 & 방향전환 커맨드 정의
public class MoveCommand : ICommand
{
    CharacterController _controller;
    Vector2 _input;

    public MoveCommand(CharacterController controller, Vector2 input)
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
    CharacterController _controller;
    Vector2 _input;

    public LookCommand(CharacterController controller, Vector2 input)
    {
        _controller = controller;
        _input = input;
    }

    public void Execute()
    {
        _controller.Look(_input);
    }
}