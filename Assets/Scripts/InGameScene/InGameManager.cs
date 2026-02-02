using Photon.Pun;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameManager : MonoBehaviourPunCallbacks
{
    public static InGameManager Instance;

    [SerializeField] CinemachineCamera _survivorCameraPrefab;
    [SerializeField] CinemachineCamera _killerCameraPrefab;
    [SerializeField] GameObject _generatorPrefab;

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

        // ===== 발전기 스폰 (마스터만) =====
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnGenerator();
        }

        isReady = true;
        Debug.Log("InGameManager Ready");
    }

    private void SpawnGenerator()
    {
        PhotonNetwork.Instantiate(
            _generatorPrefab.name,
            new Vector3(10f, 0f, 10f),
            Quaternion.identity
        );
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
