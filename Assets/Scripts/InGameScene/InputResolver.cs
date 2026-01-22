using UnityEngine;
using UnityEngine.Windows;

// 현재 캐릭터가 킬러인지 생존자인지에 따라 입력 의도를 구분해서 알맞은 커맨드 반환하는 역할.
public class InputResolver : MonoBehaviour
{
    private PlayerType _inputType;
    private KillerController _killerController;
    private SurvivorController _survivorController;

    private void Start()
    {
        _inputType = CharacterStateManager.Instance.PlayerType;
        SetController();
    }

    public ICommand OnMove(Vector2 input)
    {
        switch (_inputType)
        {
            case PlayerType.Killer:
                return new MoveCommand(_killerController, input);

            case PlayerType.Survivor:
                return new MoveCommand(_survivorController, input);

            default:
                Debug.Log("잘못된 플레이어 타입 설정 : None");
                return null;
        }
    }

    public ICommand OnLook(Vector2 input) //이거 시선처리 어디선가 로직 분리해 줘야 함
    {
        switch (_inputType)
        {
            case PlayerType.Killer:
                return new LookCommand(_killerController, input);

            case PlayerType.Survivor:
                return new LookCommand(_survivorController, input);

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
                return null;
                
                default:
                Debug.Log("잘못된 플레이어 타입 설정 : None");
                return null;
        }
    }

    public ICommand OnLeftMouseHold()
    {
        switch (_inputType)
        {
            case PlayerType.Killer:
                return new LungeAttackCommand(_killerController);


            case PlayerType.Survivor:
                return new InteractCommand(_survivorController);

            default:
                Debug.Log("잘못된 플레이어 타입 설정 : None");
                return null;
        }
    }

    public ICommand OnRightMouseClick()
    {
        switch (_inputType)
        {
            case PlayerType.Killer:
                return new LungeAttackCommand(_killerController); //이거 특수능력으로 바꿔주기


            case PlayerType.Survivor:
                return new InteractCommand(_survivorController); //이거 아이템 사용으로 바꿔주기

            default:
                Debug.Log("잘못된 플레이어 타입 설정 : None");
                return null;
        }
    }

    public ICommand OnRightMouseHold()
    {
        switch (_inputType)
        {
            case PlayerType.Killer:
                return new LungeAttackCommand(_killerController); //이거 특수능력1로 바꿔주기


            case PlayerType.Survivor:
                return new InteractCommand(_survivorController); //이거 아이템 사용으로 바꿔주기

            default:
                Debug.Log("잘못된 플레이어 타입 설정 : None");
                return null;
        }
    }

    public ICommand OnCtrl()
    {
        switch (_inputType)
        {
            case PlayerType.Killer:
                return new LungeAttackCommand(_killerController); //이거 특수능력2로 바꿔주기


            case PlayerType.Survivor:
                return new SitCommand(_survivorController);

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
                return new VaultCommand(_killerController); //이거 판자부수기, 발전기 부수기랑 통합했다가 컨트롤러딴(혹은 상황별 액션 분리기)에서 나눠줘야함.


            case PlayerType.Survivor:
                return new VaultCommand(_survivorController);

            default:
                Debug.Log("잘못된 플레이어 타입 설정 : None");
                return null;
        }
    }

    public ICommand OnR()
    {
        switch (_inputType)
        {
            case PlayerType.Killer:
                return new DropCommand(_killerController); //이거 판자부수기, 발전기 부수기랑 통합했다가 컨트롤러딴(혹은 상황별 액션 분리기)에서 나눠줘야함.


            case PlayerType.Survivor:
                return new VaultCommand(_survivorController); //이거 아이템 버리기로 바꿔줘야 함

            default:
                Debug.Log("잘못된 플레이어 타입 설정 : None");
                return null;
        }
    }

    private void SetController()
    {
        switch (_inputType)
        {
            case PlayerType.Killer:
                _killerController = new KillerController();
                break;

            case PlayerType.Survivor:
                _survivorController = new SurvivorController();
                break;
        }
    }
}
