using UnityEngine;
using Photon.Pun;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    public bool IsServerConnected { get; private set; }
    public bool IsJoinedLobby { get; private set; }


    private void Awake()
    {
        Instance = this;
        IsServerConnected = false;
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
        IsServerConnected = true;
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
