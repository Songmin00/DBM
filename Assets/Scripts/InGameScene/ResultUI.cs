using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ResultUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _resultText;    
    [SerializeField] private Button _lobbyButton;

    private void Start()
    {
        string winner = PlayerPrefs.GetString("Winner", "None");

        if (winner == "Survivor")
        {
            _resultText.text = "생존자의 승리!";
            _resultText.color = Color.cyan;            
        }
        else
        {
            _resultText.text = "킬러의 승리!";
            _resultText.color = Color.red;
        }

        _lobbyButton.onClick.AddListener(() => {
            // 로비로 돌아갈 때 인게임 룸 퇴장하기
            PhotonNetwork.LeaveRoom();
            SoundManager.Instance.PlayBGM("LobbyBGM", 0.3f);
            SceneManager.LoadScene("LobbyScene");
        });
    }
}