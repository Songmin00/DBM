using UnityEngine;
using UnityEngine.Windows;

// 현재 캐릭터가 킬러인지 생존자인지에 따라 입력 의도를 구분해서 알맞은 커맨드 반환하는 역할.
public class InputTypeResolver
{
    private PlayerType _inputType;

    private KillerInputActionResolver _killerInputActionResolver;
    private SurvivorInputActionResolver _survivorInputActionResolver;

    private KillerController _killerController;
    private SurvivorController _survivorController;

    public InputTypeResolver(PlayerType playerType)
    {
        _inputType = playerType;

        switch (_inputType)
        {
            case PlayerType.Killer:
                _killerController = InGameManager.Instance.GetCharacterObject().GetComponent<KillerController>();
                _killerInputActionResolver = new KillerInputActionResolver(_killerController);
                break;

            case PlayerType.Survivor:
                _survivorController = InGameManager.Instance.GetCharacterObject().GetComponent<SurvivorController>();
                _survivorInputActionResolver = new SurvivorInputActionResolver(_survivorController);
                break;
        }
    }

    public ICommand OnWASD(Vector2 input)
    {
        switch (_inputType)
        {
            case PlayerType.Killer:
                return _killerInputActionResolver.ResolveMove(input);

            case PlayerType.Survivor:
                return _survivorInputActionResolver.ResolveMove(input);

            default:
                Debug.Log("잘못된 플레이어 타입 설정 : None");
                return null;
        }
    }

    public ICommand OnMousePointer(Vector2 input) //캐릭터 오브젝트 회전용. 카메라 이동은 따로 처리
    {
        switch (_inputType)
        {
            case PlayerType.Killer:
                return _killerInputActionResolver.ResolveLook(input);

            case PlayerType.Survivor:
                return _survivorInputActionResolver.ResoleveLook(input);

            default:
                Debug.Log("잘못된 플레이어 타입 설정 : None");
                return null;
        }
    }

    public ICommand OnLeftMouseClick()
    {
        switch (_inputType)
        {
            case PlayerType.Killer:
                return new AttackCommand(_killerController);

            case PlayerType.Survivor:
                return new NullCommand();

            default:
                Debug.Log("잘못된 플레이어 타입 설정 : None");
                return null;
        }
    }

    public ICommand OnLeftMouseHold(bool isHold)
    {
        switch (_inputType)
        {
            case PlayerType.Killer:
                return new LungeAttackCommand(_killerController);

            case PlayerType.Survivor:
                return new SurvivorInteractCommand(_survivorController, isHold);

            default:
                Debug.Log("잘못된 플레이어 타입 설정 : None");
                return null;
        }
    }

    public ICommand OnCtrl(bool isHold)
    {
        switch (_inputType)
        {
            case PlayerType.Killer:
                return new NullCommand(); //이거 특수능력2로 바꿔주기

            case PlayerType.Survivor:
                return _survivorInputActionResolver.ResolveSit(isHold);

            default:
                Debug.Log("잘못된 플레이어 타입 설정 : None");
                return null;
        }
    }

    public ICommand OnSpace()
    {
        switch (_inputType)
        {
            case PlayerType.Killer:
                return new KillerInteractCommand(_killerController);

            case PlayerType.Survivor:
                return new NullCommand();

            default:
                Debug.Log("잘못된 플레이어 타입 설정 : None");
                return null;
        }
    }

    public ICommand OnShift(bool isHold)
    {
        switch (_inputType)
        {
            case PlayerType.Killer:
                return new NullCommand();

            case PlayerType.Survivor:
                return new RunCommand(_survivorController, isHold);

            default:
                Debug.Log("잘못된 플레이어 타입 설정 : None");
                return null;
        }
    }
}
