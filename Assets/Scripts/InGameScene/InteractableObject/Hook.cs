using Photon.Pun;
using UnityEngine;

public class Hook : MonoBehaviour, IKillerInteractable, ISurvivorInteractable
{
    [SerializeField] private Transform _hookPoint;

    public bool IsKillerInteractable { get; set; } = true;
        
    public bool IsSurvivorInteractable
    {
        get => true; // 항상 true로 두어 탐색 대상에 포함되게 함
        set { }
    }

    public SurvivorStateManager OccupyingSurvivor { get; private set; }

    private void Awake()
    {
        // 기본적으로 생존자가 구조할 수 있는 상태로 설정
        IsSurvivorInteractable = true;
    }

    public void StartKillerInteract(KillerController killer)
    {
        SurvivorController carriedSurvivor = killer.GetLiftPoint().GetComponentInChildren<SurvivorController>();
        if (carriedSurvivor != null)
        {
            var pv = carriedSurvivor.GetComponent<PhotonView>();
            if (pv != null)
            {
                OccupyingSurvivor = carriedSurvivor.GetComponent<SurvivorStateManager>();
                pv.RPC("RPC_AttachToHook", RpcTarget.AllBuffered, GetComponent<PhotonView>().ViewID);
            }
        }
    }

    public void StartSurvivorInteract() { }
    public void StopSurvivorInteract() { }
    public void StopKillerInteract() { }

    public Transform GetHookPoint() => _hookPoint;

    public void OnRescueComplete()
    {
        OccupyingSurvivor = null;
    }
}