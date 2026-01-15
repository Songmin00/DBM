using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


public class AuthManager : MonoBehaviour
{
    public static FirebaseUser User { get; set; } //인증이 다 되고 나서 인증된 유저 정보를 들고 있도록 하는 것. 웹개발의 토큰 역할?
    public static DatabaseReference DbRef { get; set; }

    private AuthUIController _authUIController;    
    
    public FirebaseAuth Auth { get; set; }



    private void Awake()
    {
        _authUIController = GetComponent<AuthUIController>();

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == Firebase.DependencyStatus.Available) // 가능하다는 결과를 받았으면?
            {
                Auth = Firebase.Auth.FirebaseAuth.DefaultInstance; //인증 정보를 기억시킴
                DbRef = FirebaseDatabase.DefaultInstance.RootReference; //모든 데이터의 루트 정보(짬뽕 정보)를 가져와서 dbRef에 저장
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format("에러 발생" + task.Result)); //실패시 로그 띄우기
            }
        });
    }

    private void Start()
    {
        if (SaveManager.Instance.User == null)
        {
            _authUIController.SetStartButtonAvailable(false);
        }
    }

    public void Login()
    {
        StartCoroutine(LogInRoutine());
    }


    private IEnumerator LogInRoutine()
    {
        if (Auth == null)
        {
            _authUIController.SetFailText("Firebase 초기화 중입니다. 잠시만 기다려주세요.");
            yield break;
        }

        Task<AuthResult> logInTask = Auth.SignInWithEmailAndPasswordAsync(_authUIController.GetEMail(), _authUIController.GetPassword());

        yield return new WaitUntil(predicate: () => logInTask.IsCompleted);

        if (logInTask.Exception != null) //로그인에서 문제가 발생했다면
        {
            Debug.Log($"로그인 실패. 원인 : {logInTask.Exception}");

            //FireBase에서는 에러를 분석할 수 있는 형식을 제공하고 있음.

            FirebaseException firebaseEx = logInTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = string.Empty;

            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "이메일을 입력해주세요.";
                    break;
                case AuthError.MissingPassword:
                    message = "패스워드를 입력해주세요.";
                    break;
                case AuthError.WrongPassword:                    
                case AuthError.InvalidCredential:
                    message = "패스워드가 올바르지 않습니다.";
                    break;
                case AuthError.Failure:             
                    message = "이메일 또는 패스워드가 올바르지 않습니다.";
                    break;
                case AuthError.InvalidEmail:
                    message = "이메일이 올바르지 않습니다.";
                    break;
                case AuthError.UserNotFound:
                    message = "존재하지 않는 계정입니다.";
                    break;
                default:
                    message = "로그인 오류 발생. 관리자에게 문의해주세요.";
                    break;
            }
            Debug.Log($"로그인 실패. 에러코드 : {errorCode.ToString()}");
            _authUIController.SetFailText(message);

        }
        else
        {
            User = logInTask.Result.User;            
            SaveManager.Instance.SetUserData();
            _authUIController.SetConfirmText($"환영합니다, {User.DisplayName} 님");            
            _authUIController.SetStartButtonAvailable(true);
            SaveManager.Instance.LoadFromDB();
        }
    }


    public void Register()
    {
        StartCoroutine(RegisterRoutine());
    }



    IEnumerator RegisterRoutine()
    {
        if (Auth == null)
        {
            _authUIController.SetFailText("Firebase 초기화 중입니다. 잠시만 기다려주세요.");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(_authUIController.GetNickName()))
        {
            _authUIController.SetFailText("닉네임을 입력해주세요.");
            yield break;
        }

        string pw = _authUIController.GetPassword();

        if (!IsValidPassword(pw))
        {
            _authUIController.SetFailText(
                "패스워드는 영어 대문자, 소문자, 숫자를 포함해 6자 이상이어야 합니다."
            );
            yield break;
        }

        Task<AuthResult> registerTask = Auth.CreateUserWithEmailAndPasswordAsync(_authUIController.GetEMail(), _authUIController.GetPassword());

        yield return new WaitUntil(predicate: () => registerTask.IsCompleted);


        if (registerTask.Exception != null)
        {
            Debug.Log(message: "실패 사유" + registerTask.Exception);
            FirebaseException firebaseEx = registerTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "회원가입 실패";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "이메일을 설정해주세요.";
                    break;
                case AuthError.MissingPassword:
                    message = "패스워드를 설정해주세요.";
                    break;
                case AuthError.WeakPassword:
                    message = "패스워드는 영어 대문자, 소문자, 숫자를 포함하여 6자 이상이어야 합니다.";
                    break;
                case AuthError.EmailAlreadyInUse:
                    message = "이미 등록된 이메일입니다.";
                    break;
                case AuthError.InvalidEmail:
                    message = "이메일 형식이 올바르지 않습니다.";
                    break;                
                default:
                    message = "회원가입 오류 발생. 관리자에게 문의해주세요."; ;
                    break;
            }
            _authUIController.SetFailText(message);
        }

        else
        {
            User = registerTask.Result.User;

            if (User != null)
            {
                Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile { DisplayName = _authUIController.GetNickName() }; // 로컬에서 생성한 거
                Task profileTask = User.UpdateUserProfileAsync(profile); // DB에 올리기

                yield return new WaitUntil(predicate: () => profileTask.IsCompleted); // 원격 전송이니까 완료할 때까지 기다려주기

                if (profileTask.Exception != null) // 문제가 있다면
                {
                    Debug.Log("닉네임 설정 실패 " + profileTask.Exception);
                    FirebaseException firebaseEx = profileTask.Exception.GetBaseException() as FirebaseException;
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                    _authUIController.SetFailText("닉네임 설정 실패");                    
                }
                else  //파이어베이스 기본값은 회원가입 성공하면 자동 로그인 처리
                {
                    SaveManager.Instance.SetUserData();                    
                    _authUIController.SetConfirmText($"회원가입 완료. 반갑습니다 {User.DisplayName} 님");                    
                    _authUIController.SetStartButtonAvailable(true);
                    SaveManager.Instance.LoadFromDB();
                }
            }
        }
    }

    bool IsValidPassword(string pw)
    {
        if (pw.Length < 6) return false;
        if (!pw.Any(char.IsLower)) return false;
        if (!pw.Any(char.IsUpper)) return false;
        if (!pw.Any(char.IsDigit)) return false;
        return true;
    }

    public FirebaseUser GetUserInfo()
    {
        return User;
    }
}
