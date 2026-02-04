using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameManager : MonoBehaviourPunCallbacks
{
    public static InGameManager Instance;

    [Header("맵 세팅용 필드")]
    [SerializeField] CinemachineCamera _survivorCameraPrefab;
    [SerializeField] CinemachineCamera _killerCameraPrefab;
    [SerializeField] GameObject _generatorPrefab;
    [SerializeField] GameObject _hookPrefab;
    [SerializeField] List<Transform> _generatorPos;
    [SerializeField] List<Transform> _hookPos;
    [SerializeField] Transform _killerSpawnPoint;
    [SerializeField] List<Transform> _survivorSpawnPoint;

    [Header("인게임 상황 관리용 필드")]    
    [SerializeField] int _targetGeneratorCount = 5; // 목표 개수
    private int _currentFixedCount = 0; // 현재 고쳐진 개수

    List<Generator> _generators = new List<Generator>();
    List<Hook> _hooks = new List<Hook>();


    private GameObject _playerPrefab;
    private GameObject _localInstance;
    private Vector3 _spawnPos = new Vector3(0, 1.5f, 0);

    public bool isReady { get; private set; } = false;

    private void Awake()
    {
        // 씬 전용 싱글톤
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
        SoundManager.Instance.PlayBGM("InGameBGM", 0.4f);
    }

    private IEnumerator SpawnRoutine()
    {
        
        yield return new WaitUntil(() => PhotonNetwork.InRoom);
        yield return new WaitUntil(() => CharacterStateManager.Instance.CharacterPrefab != null);

        Debug.Log("InGameManager Spawn Start");

        _playerPrefab = CharacterStateManager.Instance.CharacterPrefab;

        
        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        
        if (CharacterStateManager.Instance.PlayerType == PlayerType.Killer)
        {            
            if (_killerSpawnPoint != null)
            {
                spawnPosition = _killerSpawnPoint.position;
                spawnRotation = _killerSpawnPoint.rotation;
            }
        }
        else
        {            
            int myIndex = 0;
            int survivorCount = 0;

            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player.IsLocal)
                {
                    myIndex = survivorCount;
                    break;
                }
                survivorCount++;
            }
            
            int spawnIdx = myIndex % _survivorSpawnPoint.Count;
            spawnPosition = _survivorSpawnPoint[spawnIdx].position;
            spawnRotation = _survivorSpawnPoint[spawnIdx].rotation;
        }
        
        _localInstance = PhotonNetwork.Instantiate(_playerPrefab.name, spawnPosition, spawnRotation);
        

        if (CharacterStateManager.Instance.PlayerType == PlayerType.Survivor)
        {
            CinemachineCamera cam = Instantiate(_survivorCameraPrefab);
            SurvivorController sc = _localInstance.GetComponent<SurvivorController>();
            cam.Follow = sc.GetCameraAnchor();
            cam.LookAt = sc.GetCameraAnchor();
        }
        else if (CharacterStateManager.Instance.PlayerType == PlayerType.Killer)
        {
            CinemachineCamera cam = Instantiate(_killerCameraPrefab);
            KillerController kc = _localInstance.GetComponent<KillerController>();
            cam.Follow = kc.GetCameraAnchor();
            cam.LookAt = kc.GetCameraTarget();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            SpawnGenerator();
            SpawnHook();
        }

        yield return new WaitUntil(() => InGameUIManager.Instance != null);
        InGameUIManager.Instance.UpdateGeneratorUI(_targetGeneratorCount);

        isReady = true;
        Debug.Log($"InGameManager Ready - Spawned at {spawnPosition}");
    }

    private void SpawnGenerator()
    {
        // 마스터 클라이언트가 발전기 스폰
        foreach (var pos in _generatorPos)
        {
            GameObject go = PhotonNetwork.Instantiate(_generatorPrefab.name, pos.position, pos.rotation);
            _generators.Add(go.GetComponent<Generator>());
        }
        
        if (_targetGeneratorCount <= 0) _targetGeneratorCount = _generators.Count;
    }

    public void OnGeneratorFixed()
    {        
        if (!PhotonNetwork.IsMasterClient) return;

        _currentFixedCount++;
        int remaining = Mathf.Max(0, _targetGeneratorCount - _currentFixedCount);
     
        photonView.RPC(nameof(RPC_UpdateUIAndCheckWin), RpcTarget.All, remaining);
    }

    [PunRPC]
    private void RPC_UpdateUIAndCheckWin(int remaining)
    {        
        InGameUIManager.Instance.UpdateGeneratorUI(remaining);
        
        if (remaining <= 0)
        {            
            photonView.RPC(nameof(RPC_GameOver), RpcTarget.All, "Survivor");
        }
    }
    // 마스터 클라이언트에서 주기적으로 혹은 중요한 사건 발생 시 호출
   public void CheckGameOver()
{
    if (!PhotonNetwork.IsMasterClient) return;
    
    SurvivorStateManager[] allSurvivors = FindObjectsByType<SurvivorStateManager>(FindObjectsSortMode.None);
    int totalSurvivors = allSurvivors.Length;
    int incapacitatedCount = 0;

    foreach (var survivor in allSurvivors)
    {        
        if (survivor.CurrentState == SurvivorState.Dead || survivor.CurrentState == SurvivorState.Hang)
        {
            incapacitatedCount++;
        }
    }
    
    if (incapacitatedCount >= totalSurvivors)
    {
        photonView.RPC(nameof(RPC_GameOver), RpcTarget.All, "Killer");
    }
}

    [PunRPC]
    private void RPC_GameOver(string winner)
    {
        Debug.Log($"게임 종료! 승자: {winner}");
        
        PlayerPrefs.SetString("Winner", winner);
        
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("ResultScene");
        }
    }
    private void SpawnHook()
    {
        for (int i = 0; i < _hookPos.Count; i++)
        {
            _hooks.Add(PhotonNetwork.Instantiate(_hookPrefab.name, _hookPos[i].position, Quaternion.identity).GetComponent<Hook>());
        }
        
    }


    public GameObject GetCharacterObject()
    {
        return _localInstance;
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
