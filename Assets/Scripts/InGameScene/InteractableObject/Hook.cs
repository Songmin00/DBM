using Photon.Pun;
using UnityEngine;

public class Hook : MonoBehaviour, IKillerInteractable
{
    [Header("생존자가 매달릴 지점")]
    [SerializeField] private Transform _hookPoint;

    public bool IsKillerInteractable { get; set; } = true;

    public void StartKillerInteract(KillerController killer)
    {
        // 킬러가 현재 누군가를 들고 있는지 확인 (보통 KillerController나 State에 저장)
        // 여기서는 간단히 킬러의 자식 중 SurvivorController가 있는지 확인하는 방식으로 처리
        SurvivorController carriedSurvivor = killer.GetLiftPoint().GetComponentInChildren<SurvivorController>();

        if (carriedSurvivor != null)
        {
            // 생존자에게 갈고리에 걸리라고 명령
            var stateManager = carriedSurvivor.GetComponent<SurvivorStateManager>();
            if (stateManager != null)
            {
                // 상태를 Hang으로 변경하고, 부모를 갈고리의 hookPoint로 변경
                stateManager.RequestStateChange(SurvivorState.Hang, true);
                stateManager.GetComponent<PhotonView>().RPC("RPC_AttachToHook", RpcTarget.All, _hookPoint.GetComponent<PhotonView>().ViewID);
            }

            Debug.Log("생존자를 갈고리에 걸었습니다.");
        }
    }

    public void StopKillerInteract() { }
}