using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameManager : MonoBehaviourPunCallbacks
{
    public static InGameManager Instance;

    [SerializeField] GameObject _playerPrefab;
    Vector3 _spawnPos = new Vector3(0, 1.5f, 0);


    private void Start() //씬이 너무 빨리 불러와져서 start가 룸 들어가기 전에 호출되는 문제가 있음
    {
        Instance = this;

        //if (PlayerManager.LocalPlayerInstance == null)
        {
            StartCoroutine(SpawnPlayerWhenConnected());
        }
    }

    private IEnumerator SpawnPlayerWhenConnected()
    {
        yield return new WaitUntil(() => PhotonNetwork.InRoom);
        //PlayerManager.LocalPlayerInstance = PhotonNetwork.Instantiate(_playerPrefab.name, _spawnPos, Quaternion.identity);
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}
