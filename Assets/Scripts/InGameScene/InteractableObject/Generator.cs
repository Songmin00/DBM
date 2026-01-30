using Photon.Pun;
using UnityEngine;

public class Generator : MonoBehaviourPun, IInteractable
{
    private float gauge = 80f;
    private float _currentGauge = 0f;
    private int _fixingSurvivors = 0;
    private bool _isFixed = false;

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (_isFixed) return;
        if (_fixingSurvivors <= 0) return;

        FixingLogic();
    }

    public void StartInteract()
    {
        photonView.RPC(nameof(RPC_AddFixer), RpcTarget.MasterClient);
    }

    public void StopInteract()
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

        photonView.RPC(nameof(RPC_SyncGauge), RpcTarget.Others, _currentGauge);

        if (_currentGauge >= gauge)
        {
            _currentGauge = gauge;
            GetFixed();
        }
    }

    [PunRPC]
    private void RPC_SyncGauge(float value)
    {
        _currentGauge = value;
    }

    private void GetFixed()
    {
        _isFixed = true;
        photonView.RPC(nameof(RPC_Fixed), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_Fixed()
    {
        _isFixed = true;
        Debug.Log("Fixed!!");
    }
}
