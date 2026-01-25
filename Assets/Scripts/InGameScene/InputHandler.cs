using System.Collections.Generic;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviourPunCallbacks
{
    private InputTypeResolver _inputTypeResolver;
    private PlayerType _inputType;

    List<ICommand> _commands = new List<ICommand>(); //매 프레임 실행할 커맨드 관리용 리스트
    
    

    private void Start()
    {
        _inputType = CharacterStateManager.Instance.PlayerType;
        StartCoroutine(SetTypeRoutine()); // InGameManager가 캐릭턱를 생성하기를 기다렸다가 준비되면 정보 받아와서 하위 시스템들 세팅.
    }

    private IEnumerator SetTypeRoutine()
    {
        yield return new WaitUntil(() => InGameManager.Instance != null && InGameManager.Instance.isReady);
        _inputTypeResolver = new InputTypeResolver(CharacterStateManager.Instance.PlayerType);
    }

    private void Update()
    {        
        foreach (var command in _commands)
        {
            command.Execute();
        }
        _commands.Clear();
    }

    public void OnWASD(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        _commands.Add(_inputTypeResolver.OnWASD(input));
    }

    public void OnMousePointer(InputAction.CallbackContext ctx)
    {        
        Vector2 input = ctx.ReadValue<Vector2>();
        _commands.Add(_inputTypeResolver.OnMousePointer(input));        
    }

    public void OnLeftMouseClick()
    {

    }

    public void OnLeftMouseHold()
    {

    }

    public void OnRightMouseClick()
    {

    }

    public void OnRightMouseHold()
    {

    }

    public void OnCtrl()
    {

    }

    public void OnSpace()
    {

    }

    public void OnR()
    {

    }

    public void OnShift()
    {

    }
}
