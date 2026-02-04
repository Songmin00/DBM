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
    public int CurrentTargetViewId => _currentTargetViewId;
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

        // 현재 타겟이 트리거 밖으로 나가면 Stop도 보내주기
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
        
        PhotonView targetPv = Nearest.GetComponentInParent<PhotonView>();
        if (targetPv == null) return;
        
        Hook hook = targetPv.GetComponent<Hook>();
        if (hook != null)
        {
            if (hook.OccupyingSurvivor != null)
            {                
                targetPv = hook.OccupyingSurvivor.GetComponent<PhotonView>();
            }
            else
            {                
                return;
            }
        }
        
        var interactable = targetPv.GetComponent<ISurvivorInteractable>();
        if (interactable == null || !interactable.IsSurvivorInteractable) return;

        _currentTargetViewId = targetPv.ViewID;

        SurvivorStateManager state = targetPv.GetComponent<SurvivorStateManager>();
        if (state != null)
        {
            _currentTargetIsSurvivorState = true;            
            targetPv.RPC("RPC_SurvivorInteractStart", targetPv.Owner, _pv.ViewID);
        }
        else
        {
            _currentTargetIsSurvivorState = false;
            interactable.StartSurvivorInteract();
        }
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
