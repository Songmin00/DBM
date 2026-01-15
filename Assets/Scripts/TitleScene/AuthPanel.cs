using TMPro;
using UnityEngine;


//로그인 패널, 회원가입 패널에 각각 부착시킬 컴포넌트
public class AuthPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField _emailField;
    [SerializeField] TMP_InputField _passwordField;
    [SerializeField] TMP_InputField _nickNameField;

    [SerializeField] TextMeshProUGUI _confirmText;
    [SerializeField] TextMeshProUGUI _failText;
    

    private void Start()
    {        
        _failText.text = string.Empty;
        _confirmText.text = string.Empty;
    }

    public string GetEmail()
    {
        return _emailField.text;
    }
    public string GetPassword()
    {
        return _passwordField.text;
    }

    public string GetNickName()
    {
        if (_nickNameField == null)
        {
            return "닉네임 필드가 없습니다.";
        }
        return _nickNameField.text;
    }

    public void SetFailText(string text)
    {
        _confirmText.text = string.Empty;
        _failText.text = text;
    }

    public void SetConfirmText(string text)
    {
        _failText.text = string.Empty;
        _confirmText.text = text;
    }
}
