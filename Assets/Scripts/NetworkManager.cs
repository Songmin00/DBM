using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class NetworkManager : Pungleton<NetworkManager>
{
    public bool IsJoinedLobby { get; private set; }


    protected override void Awake()
    {
        base.Awake();
        IsJoinedLobby = false;
        PhotonNetwork.AutomaticallySyncScene = true;
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "kr";
        PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("서버 연결 완료");        
    }

    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();        
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("로비 연결 완료. 로비 씬으로 이동합니다.");
        IsJoinedLobby = true;
    }
}
