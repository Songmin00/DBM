using Photon.Pun;
using UnityEngine;

public class Generator : MonoBehaviourPun, ISurvivorInteractable, IPunObservable
{
    private float _totalGauge = 80f;
    private float _currentGauge = 0f;
    private int _fixingSurvivors = 0;

    public bool IsSurvivorInteractable { get; set; } = true;

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!IsSurvivorInteractable) return;
        if (_fixingSurvivors <= 0) return;

        FixingLogic();
    }

    public void StartSurvivorInteract()
    {
        photonView.RPC(nameof(RPC_AddFixer), RpcTarget.MasterClient);
    }

    public void StopSurvivorInteract()
    {
        photonView.RPC(nameof(RPC_RemoveFixer), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RPC_AddFixer()
    {
        _fixingSurvivors++;
    }

    [PunRPC]
    private void RPC_RemoveFixer()
    {
        _fixingSurvivors = Mathf.Max(0, _fixingSurvivors - 1);
    }

    private void FixingLogic()
    {
        _currentGauge += Time.deltaTime * (1 + (_fixingSurvivors - 1) * 0.5f);

        Debug.Log($"ÁøÇàµµ : {_currentGauge}");
        if (_currentGauge >= _totalGauge)
        {
            _currentGauge = _totalGauge;
            GetFixed();
        }
    }

    private void GetFixed()
    {
        IsSurvivorInteractable = false;
        photonView.RPC(nameof(RPC_Fixed), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_Fixed()
    {
        IsSurvivorInteractable = false;
        Debug.Log("Fixed!!");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_currentGauge);
        }
        else
        {
            _currentGauge = (float)stream.ReceiveNext();
        }
    }
}
