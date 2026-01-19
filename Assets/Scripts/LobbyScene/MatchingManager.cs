using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;



public class MatchingManager : MonoBehaviourPunCallbacks
{
    // [테스트용 UI 및 디버깅]
    [SerializeField] GameObject TestUI;
    [SerializeField] TextMeshProUGUI DebugingUI;

    // [매칭 설정]
    private int _requiredKiller = 1;
    private int _requiredSurvivor = 2;
    private int _maxIngamePlayer = 3;

    private const string _matchingRoomName = "MatchingRoom";
    private string _inGameRoomName; // 매칭 성공 시 이동할 방 이름 저장용

    public PlayerType playerType { get; private set; } = PlayerType.None;

    // [룸 옵션 설정]
    private RoomOptions _matchingRoomOptions = new RoomOptions { MaxPlayers = 50, IsOpen = true, IsVisible = false };
    private RoomOptions _inGameRoomOptions;

    private void Awake()
    {
        _inGameRoomOptions = new RoomOptions { MaxPlayers = (byte)_maxIngamePlayer, IsOpen = true, IsVisible = true };
    }

    #region 버튼 연결용 퍼블릭 함수
    public void OnKillerButtonClick()
    {
        playerType = PlayerType.Killer;
        StartMatching();
    }

    public void OnSurvivorButtonClick()
    {
        playerType = PlayerType.Survivor;
        StartMatching();
    }
    #endregion

    // 1. 매칭 서버(방) 접속 시작
    private void StartMatching()
    {
        if (PhotonNetwork.IsConnected)
        {
            DebugingUI.text = "매칭룸 접속 중...";
            PhotonNetwork.JoinOrCreateRoom(_matchingRoomName, _matchingRoomOptions, TypedLobby.Default);
        }
        else
        {
            DebugingUI.text = "네트워크 연결 확인 필요";
        }
    }

    // 2. 방 입장 완료 (매칭룸 or 인게임룸)
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.Name == _matchingRoomName)
        {
            // [매칭룸 입장 시] 자신의 역할을 프로퍼티에 업로드
            ExitGames.Client.Photon.Hashtable playerProp = new ExitGames.Client.Photon.Hashtable();
            playerProp["PlayerType"] = playerType.ToString();
            PhotonNetwork.SetPlayerCustomProperties(playerProp);

            DebugingUI.text = $"{playerType} 역할로 대기 중...";
        }
        else
        {
            // [인게임룸 입장 시] 게임 UI 활성화
            TestUI.SetActive(true);
            DebugingUI.text = "인게임 입장 완료!";
        }
    }

    // 3. 누군가의 역할 정보가 갱신될 때마다 마스터 클라이언트가 매칭 가능 여부 확인
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!PhotonNetwork.IsMasterClient) return; //마스터가 아니면 리턴.
        if (PhotonNetwork.CurrentRoom.Name != _matchingRoomName) return; //매칭룸이 아니면 리턴.
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("InGameRoomName")) return; // 이미 매칭 완료 시 중단

        CheckMatchable();
    }

    // 플레이어가 나갔을 때도 인원수를 다시 체크
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.Name == _matchingRoomName) //마스터만 발동 && 매칭룸에서만 발동
        {
            CheckMatchable();
        }
    }

    // [매칭 판별 로직]
    private void CheckMatchable()
    {
        List<Player> killers = new List<Player>();
        List<Player> survivors = new List<Player>();

        foreach (var p in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (p.CustomProperties.TryGetValue("PlayerType", out object type))
            {
                if (type.ToString() == "Killer") killers.Add(p);
                else if (type.ToString() == "Survivor") survivors.Add(p);
            }
        }

        if (killers.Count >= _requiredKiller && survivors.Count >= _requiredSurvivor)
        {
            DebugingUI.text = "매칭 성공! 방 정보 생성 중...";
            ExecuteMatch(killers.Take(_requiredKiller).ToList(), survivors.Take(_requiredSurvivor).ToList());
        }
        else
        {
            DebugingUI.text = $"대기 중 (K: {killers.Count}/{_requiredKiller}, S: {survivors.Count}/{_requiredSurvivor})";
        }
    }

    // [매칭 실행] 룸 프로퍼티에 이동할 방 이름과 인원 명단 배포
    private void ExecuteMatch(List<Player> selectedKillers, List<Player> selectedSurvivors)
    {
        ExitGames.Client.Photon.Hashtable roomProp = new ExitGames.Client.Photon.Hashtable();
        List<Player> finalMembers = new List<Player>();
        finalMembers.AddRange(selectedKillers);
        finalMembers.AddRange(selectedSurvivors);

        for (int i = 0; i < finalMembers.Count; i++)
        {
            roomProp[i.ToString()] = finalMembers[i].ActorNumber;
        }

        string inGameRoomName = $"Game_{PhotonNetwork.LocalPlayer.ActorNumber}_{Random.Range(100, 999)}";
        roomProp["InGameRoomName"] = inGameRoomName;

        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProp);
    }

    // 4. 룸 프로퍼티 변경 감지 (이동 명령 수신)
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (!propertiesThatChanged.ContainsKey("InGameRoomName")) return;

        bool isMatched = false;
        for (int i = 0; i < _maxIngamePlayer; i++)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(i.ToString(), out object actorNum))
            {
                if (actorNum.ToString() == PhotonNetwork.LocalPlayer.ActorNumber.ToString())
                {
                    isMatched = true;
                    break;
                }
            }
        }

        if (isMatched)
        {
            // 이동할 방 이름을 저장하고 현재 매칭룸 탈퇴
            _inGameRoomName = PhotonNetwork.CurrentRoom.CustomProperties["InGameRoomName"].ToString();
            DebugingUI.text = "매칭 확인됨. 인게임으로 이동합니다...";
            PhotonNetwork.LeaveRoom();
        }
    }

    
    // 5. 매칭룸을 완전히 나갔을 때 실행
    public override void OnLeftRoom()
    {
        // 방을 나갔을 때 바로 접속하지 않고, 마스터 서버로 복귀될 때까지 대기 메시지만 띄움
        if (!string.IsNullOrEmpty(_inGameRoomName))
        {
            DebugingUI.text = "서버 복귀 중... 잠시만 기다려주세요.";
        }
    }

    // 6. 마스터 서버(로비 전 단계)에 다시 연결되었을 때 실행
    public override void OnConnectedToMaster()
    {
        // 이동할 목적지 방 이름이 저장되어 있다면, 이때가 접속의 최적기임!
        if (!string.IsNullOrEmpty(_inGameRoomName))
        {
            DebugingUI.text = $"인게임 룸 접속 시도: {_inGameRoomName}";

            // 룸 옵션 재설정 (OnLeftRoom에서 옵션 변수가 날아갈 수 있으므로 여기서 직접 선언하거나 전역변수 사용)
            RoomOptions options = new RoomOptions { MaxPlayers = (byte)_maxIngamePlayer, IsOpen = true, IsVisible = true };

            // 실제 접속 명령
            PhotonNetwork.JoinOrCreateRoom(_inGameRoomName, options, TypedLobby.Default);

            // 접속 명령을 내렸으므로 변수 초기화 (중복 실행 방지)
            _inGameRoomName = null;
        }
    }

    // 7. 만약 입장 실패 시 에러 로그 확인용
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        DebugingUI.text = $"입장 실패: {message}";
        // 실패했다면 여기서 로비화면으로 보내기.
    }
}