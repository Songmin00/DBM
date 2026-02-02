using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class SurvivorInteractManager : MonoBehaviour
{
    public Collider Nearest { get; private set; }

    private readonly List<Collider> _overlaps = new List<Collider>();
    private PhotonView _pv;

    // 현재 상호작용 중인 타겟 추적
    private int _currentTargetViewId = -1;
    private bool _currentTargetIsSurvivorState = false;

    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (_pv == null || !_pv.IsMine) return;
        Nearest = GetNearestInTrigger();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("SurvivorInteractRange")) return;
        if (!_overlaps.Contains(other))
        {
            _overlaps.Add(other);
            Debug.Log("상호작용 대상 발견");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("SurvivorInteractRange")) return;

        _overlaps.Remove(other);
        Debug.Log("상호작용 대상 제거");

        // 현재 타겟이 트리거 밖으로 나가면 Stop도 보내준다
        if (_currentTargetViewId != -1)
        {
            PhotonView targetPv = PhotonView.Find(_currentTargetViewId);
            if (targetPv == null || other.GetComponentInParent<PhotonView>() == targetPv)
            {
                SendStopToCurrentTarget();
            }
        }
    }

    public void StartInteract()
    {
        if (_pv == null || !_pv.IsMine) return;
        if (Nearest == null) return;

        // Range 콜라이더는 자식일 가능성이 높음
        PhotonView targetPv = Nearest.GetComponentInParent<PhotonView>();
        if (targetPv == null) return;

        // 상호작용 가능 여부는 대상에서 판단
        var interactable = Nearest.GetComponentInParent<ISurvivorInteractable>();
        if (interactable == null) return;
        if (!interactable.IsSurvivorInteractable) return;

        _currentTargetViewId = targetPv.ViewID;

        // 대상이 SurvivorStateManager(치료/구조)면: 대상 오너에게 RPC
        SurvivorStateManager state = targetPv.GetComponent<SurvivorStateManager>();
        if (state != null)
        {
            _currentTargetIsSurvivorState = true;
            targetPv.RPC(nameof(SurvivorStateManager.RPC_SurvivorInteractStart), targetPv.Owner, _pv.ViewID);
            return;
        }

        // 그 외(발전기 등)는: 로컬에서 Start 호출 → 내부에서 RPC 처리하도록(Generator 방식)
        _currentTargetIsSurvivorState = false;
        interactable.StartSurvivorInteract();
    }

    public void StopInteract()
    {
        if (_pv == null || !_pv.IsMine) return;
        if (_currentTargetViewId == -1) return;

        SendStopToCurrentTarget();
    }

    private void SendStopToCurrentTarget()
    {
        PhotonView targetPv = PhotonView.Find(_currentTargetViewId);
        if (targetPv != null)
        {
            if (_currentTargetIsSurvivorState)
            {
                targetPv.RPC(nameof(SurvivorStateManager.RPC_SurvivorInteractStop), targetPv.Owner, _pv.ViewID);
            }
            else
            {
                // 발전기 등: 로컬 Stop 호출(Generator가 Master로 RPC 보냄)
                var interactable = targetPv.GetComponentInParent<ISurvivorInteractable>();
                if (interactable != null)
                    interactable.StopSurvivorInteract();
            }
        }

        _currentTargetViewId = -1;
        _currentTargetIsSurvivorState = false;
    }

    private Collider GetNearestInTrigger()
    {
        Collider nearest = null;
        float minDist = float.MaxValue;

        for (int i = _overlaps.Count - 1; i >= 0; i--)
        {
            Collider col = _overlaps[i];
            if (col == null)
            {
                _overlaps.RemoveAt(i);
                continue;
            }

            var interactable = col.GetComponentInParent<ISurvivorInteractable>();
            if (interactable == null) continue;
            if (!interactable.IsSurvivorInteractable) continue;

            float dist = Vector3.SqrMagnitude(col.transform.position - transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = col;
            }
        }

        return nearest;
    }
}
