using Photon.Pun;
using System.Collections.Generic;
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

    [Header("인게임 상황 관리용 필드")]
    [SerializeField] int _generatorsToGetOut;    
    
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
    }

    private System.Collections.IEnumerator SpawnRoutine()
    {
        // Photon 룸 입장까지 대기
        yield return new WaitUntil(() => PhotonNetwork.InRoom);

        Debug.Log("InGameManager Spawn Start");

        // 캐릭터 프리팹
        _playerPrefab = CharacterStateManager.Instance.CharacterPrefab;
        if (_playerPrefab == null)
        {
            Debug.LogError("CharacterPrefab is null");
            yield break;
        }

        // ===== 캐릭터 스폰 =====
        _localInstance = PhotonNetwork.Instantiate(
            _playerPrefab.name,
            _spawnPos,
            Quaternion.identity
        );

        Debug.Log("Player Spawned : " + _localInstance.name);

        // ===== 카메라 스폰 =====
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

        // ===== 상호작용 오브젝트 스폰 (마스터만) =====
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnGenerator();
            SpawnHook();
        }

        isReady = true;
        Debug.Log("InGameManager Ready");
    }

    private void SpawnGenerator()
    {
        foreach (var pos in _generatorPos)
        {
            GameObject go = PhotonNetwork.Instantiate(_generatorPrefab.name, pos.position, pos.rotation);
            _generators.Add(go.GetComponent<Generator>());
        }
    }

    // 발전기가 고쳐질 때마다 호출될 함수
    public void OnGeneratorFixed()
    {
        _generatorsToGetOut--;
        Debug.Log($"남은 발전기: {_generatorsToGetOut}");
        if (_generatorsToGetOut <= 0)
        {
            Debug.Log("탈출구가 활성화되었습니다!");
            // 탈출구 개방 로직
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
