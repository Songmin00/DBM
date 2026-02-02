using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class InputHandler : MonoBehaviourPunCallbacks
{
    private InputTypeResolver _inputTypeResolver;
    private PlayerType _inputType;

    List<ICommand> _commands = new List<ICommand>(); //매 프레임 실행할 커맨드 관리용 리스트

    private void Start()
    {
        _inputType = CharacterStateManager.Instance.PlayerType;
        StartCoroutine(SetTypeRoutine()); // InGameManager가 캐릭터를 생성하기를 기다렸다가 준비되면 정보 받아와서 하위 시스템들 세팅.
    }

    private IEnumerator SetTypeRoutine()
    {
        yield return new WaitUntil(() => InGameManager.Instance != null && InGameManager.Instance.isReady);
        _inputTypeResolver = new InputTypeResolver(CharacterStateManager.Instance.PlayerType);
    }

    private void Update()
    {
        if (_commands.Count == 0)
        {
            return;
        }
        foreach (var command in _commands)
        {
            if (command == null)
            {
                return;
            }
            command.Execute();
        }
        _commands.Clear();
    }

    public void OnWASD(InputAction.CallbackContext ctx)
    {
        if (_inputTypeResolver == null) return;
        Vector2 input = ctx.ReadValue<Vector2>();
        _commands.Add(_inputTypeResolver.OnWASD(input));
    }

    public void OnMousePointer(InputAction.CallbackContext ctx)
    {
        if (_inputTypeResolver == null) return;
        Vector2 input = ctx.ReadValue<Vector2>();
        _commands.Add(_inputTypeResolver.OnMousePointer(input));
    }

    public void OnLeftMouseClick(InputAction.CallbackContext ctx)
    {
        if (_inputTypeResolver == null) return;
        _commands.Add(_inputTypeResolver.OnLeftMouseClick());
    }

    public void OnLeftMouseHold(InputAction.CallbackContext ctx)
    {
        if (_inputTypeResolver == null) return;

        if (ctx.started)
        {
            _commands.Add(_inputTypeResolver.OnLeftMouseHold(true));
        }
        else if (ctx.canceled)
        {
            _commands.Add(_inputTypeResolver.OnLeftMouseHold(false));
        }
    }

    public void OnRightMouseClick()
    {

    }

    public void OnRightMouseHold()
    {

    }

    public void OnCtrl(InputAction.CallbackContext ctx)
    {
        if (_inputTypeResolver == null) return;

        if (ctx.started)
        {
            _commands.Add(_inputTypeResolver.OnCtrl(true));
        }
        else if (ctx.canceled)
        {
            _commands.Add(_inputTypeResolver.OnCtrl(false));
        }
    }

    public void OnSpace(InputAction.CallbackContext ctx)
    {
        if (_inputTypeResolver == null) return;

        if (ctx.performed)
        {            
            _commands.Add(_inputTypeResolver.OnSpace());
        }
    }

    public void OnR()
    {

    }

    public void OnShift(InputAction.CallbackContext ctx)
    {
        if (_inputTypeResolver == null) return;

        if (ctx.started)
        {
            _commands.Add(_inputTypeResolver.OnShift(true));
        }
        else if (ctx.canceled)
        {
            _commands.Add(_inputTypeResolver.OnShift(false));
        }
    }
}
