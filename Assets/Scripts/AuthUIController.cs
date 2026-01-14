using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AuthUIController : MonoBehaviour
{
    [SerializeField] Button _logInButton;
    [SerializeField] Button _registerButton;
    [SerializeField] Button _startButton;

    [SerializeField] AuthPanel _logInPanel;
    [SerializeField] AuthPanel _registerPanel;
    
    AuthPanel _currentPannel;

    private void Awake()
    {
        SetAuthButtonsVisible(false);
    }

    private void Start()
    {
        StartCoroutine(ConnectServerRoutine());
    }

    IEnumerator ConnectServerRoutine()
    {        
        yield return new WaitUntil(() => NetworkManager.Instance.IsServerConnected == true);

        SetAuthButtonsVisible(true);
    }


    public void SetLogInPanelVisible(bool visible)
    {
        if (visible == true)
        {
            _currentPannel = _logInPanel;
        }
        _logInPanel.gameObject.SetActive(visible);
    }

    public void SetRegisterPanelVisible(bool visible)
    {
        if (visible == true)
        {
            _currentPannel = _registerPanel;
        }
        _registerPanel.gameObject.SetActive(visible);
    }

    public void SetAuthButtonsVisible(bool visible)
    {
        _logInButton.gameObject.SetActive(visible);
        _registerButton.gameObject.SetActive(visible);
        _startButton.gameObject.SetActive(visible);
    }

    public void SetStartButtonAvailable(bool available)
    {        
        _startButton.interactable = available;
    }

    public string GetEMail()
    {
        return _currentPannel.GetEmail();
    }

    public string GetPassword()
    {
        return _currentPannel.GetPassword();
    }

    public string GetNickName()
    {        
        return _currentPannel.GetNickName();
    }

    public void SetConfirmText(string text)
    {
        _currentPannel.SetConfirmText(text);
    }

    public void SetFailText(string text)
    {
        _currentPannel.SetFailText(text);
    }

    public void OnGameStartButtonClick()
    {
        StartCoroutine(EnterLobbyRoutine());
    }

    IEnumerator EnterLobbyRoutine()
    {
        NetworkManager.Instance.JoinLobby();

        yield return new WaitUntil(() => NetworkManager.Instance.IsJoinedLobby == true);

        SceneManager.LoadScene("LobbyScene");
    }
}
