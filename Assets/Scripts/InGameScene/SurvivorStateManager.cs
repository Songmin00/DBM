using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SurvivorState
{
    None, Hurt, Dying, Lifted, Hang, Survive, Dead
}

public class SurvivorStateManager : MonoBehaviourPunCallbacks, ISurvivorInteractable, IKillerInteractable, IPunObservable
{
    private Animator _animator;
    private SurvivorController _controller;
    private PhotonView _pv;

    private int _hookCount = 0; // 갈고리에 걸린 횟수
    public SurvivorState CurrentState { get; private set; } = SurvivorState.None;

    public bool IsSitting { get; private set; }
    public bool IsInteracting { get; private set; }

    public bool IsSurvivorInteractable { get; set; } = false;
    public bool IsKillerInteractable { get; set; } = false;

    [SerializeField] private float healTime = 16f;
    private float _healGauge = 0f;
    private readonly HashSet<int> _healers = new HashSet<int>();

    [SerializeField] private float rescueTime = 1.5f;
    private float _rescueGauge = 0f;
    private readonly HashSet<int> _rescuers = new HashSet<int>();

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<SurvivorController>();
        _pv = GetComponent<PhotonView>();
    }

    private IEnumerator Start()
    {
        while (InGameUIManager.Instance == null)
        {
            yield return null;
        }

        // [수정] SaveManager(로컬)가 아니라 PhotonView의 Owner(네트워크) 닉네임을 사용합니다.
        // 이렇게 하면 모든 클라이언트가 동일한 주인의 닉네임을 UI에 등록합니다.
        string networkName = _pv.Owner.NickName;

        // 만약 NickName이 비어있다면 대안으로 DisplayName 등을 쓸 수 있지만, 
        // 보통 포톤 로그인 시 설정한 NickName을 쓰는 것이 정석입니다.
        InGameUIManager.Instance.RegisterSurvivor(_pv.ViewID, networkName);

        InGameUIManager.Instance.UpdateSurvivorStatus(_pv.ViewID, CurrentState);

        ApplyStateVisual(CurrentState);
        ApplyInteractableFlags(CurrentState);
    }

    private void Update()
    {
        if (_pv == null || !_pv.IsMine) return;

        if (CurrentState == SurvivorState.Hurt || CurrentState == SurvivorState.Dying)
            HealingLogic();

        if (CurrentState == SurvivorState.Hang)
            RescueLogic();
    }

    public void OnHit()
    {
        if (!_pv.IsMine) return;

        if (CurrentState == SurvivorState.None)
            RequestStateChange(SurvivorState.Hurt, true);
        else if (CurrentState == SurvivorState.Hurt)
            RequestStateChange(SurvivorState.Dying, true);
    }

    // 모든 클라이언트에서 공통으로 실행되어야 하는 로직 (상태값 변경 직후 호출)
    private void OnStateChanged(SurvivorState newState)
    {        

        if (newState == SurvivorState.Dead)
        {
            HandleDeath();
        }

        ApplyStateVisual(newState);
        ApplyInteractableFlags(newState);

        if (InGameUIManager.Instance != null)
        {
            InGameUIManager.Instance.UpdateSurvivorStatus(_pv.ViewID, newState);
        }
    }

    private void HandleDeath()
    {
        Debug.Log($"{_pv.Owner.NickName} 사망");

        // 물리 및 이동 차단
        _controller.SetPhysical(false);
        _controller.SetMoveSpeed(0f);

        // 마스터 클라이언트가 승리 조건 체크
        if (PhotonNetwork.IsMasterClient && InGameManager.Instance != null)
        {
            InGameManager.Instance.CheckGameOver();
        }
    }

    public void RequestStateChange(SurvivorState next, bool resetProgress)
    {        
        _pv.RPC(nameof(RPC_SyncState), RpcTarget.All, (int)next, resetProgress);
    }

    [PunRPC]
    private void RPC_SyncState(int nextState, bool resetProgress)
    {
        SurvivorState next = (SurvivorState)nextState;
        
        if (next == SurvivorState.Hang && CurrentState != SurvivorState.Hang)
        {
            _hookCount++;
            Debug.Log($"{_pv.Owner.NickName} Hook Count: {_hookCount}");
            
            if (_hookCount >= 3)
            {
                next = SurvivorState.Dead;
            }
        }

        if (CurrentState == next && next != SurvivorState.Hang) return;

        CurrentState = next;

        if (resetProgress)
        {
            _healers.Clear();
            _rescuers.Clear();
            _healGauge = 0f;
            _rescueGauge = 0f;
        }

        OnStateChanged(next);
        if (PhotonNetwork.IsMasterClient && InGameManager.Instance != null)
        {
            InGameManager.Instance.CheckGameOver();
        }
    }

    public void SetSitting(bool sit)
    {
        IsSitting = sit;
        if (_animator != null) _animator.SetBool("IsSitting", sit);
    }

    public void SetInteracting(bool interact)
    {
        IsInteracting = interact;
        if (_animator != null) _animator.SetBool("IsInteracting", interact);
    }

    public bool CanMove()
    {
        if (IsInteracting) return false;
        if (CurrentState == SurvivorState.Lifted) return false;
        if (CurrentState == SurvivorState.Hang) return false;
        if (CurrentState == SurvivorState.Dead) return false;
        return true;
    }

    public void StartSurvivorInteract() => SetInteracting(true);
    public void StopSurvivorInteract() => SetInteracting(false);

    public void StartKillerInteract(KillerController killer)
    {
        if (killer == null) return;
        PhotonView killerPv = killer.GetComponent<PhotonView>();
        if (killerPv == null || _pv == null) return;

        _pv.RPC(nameof(RPC_KillerInteract), _pv.Owner, killerPv.ViewID);
    }

    public void StopKillerInteract() { }

    [PunRPC]
    private void RPC_KillerInteract(int killerViewId)
    {
        if (!_pv.IsMine) return;

        if (CurrentState == SurvivorState.Dying)
        {
            RequestStateChange(SurvivorState.Lifted, true);
            _pv.RPC(nameof(RPC_Lift), RpcTarget.All, killerViewId);
        }
    }

    [PunRPC]
    public void RPC_AttachToHook(int hookViewId)
    {
        PhotonView hookPv = PhotonView.Find(hookViewId);
        if (hookPv == null) return;

        RequestStateChange(SurvivorState.Hang, true);

        _controller.SetPhysical(false);
        if (TryGetComponent(out PhotonTransformView ptv)) ptv.enabled = false;

        Transform targetPoint = hookPv.GetComponent<Hook>().GetHookPoint();
        transform.SetParent(null);
        transform.position = targetPoint.position;
        transform.rotation = targetPoint.rotation;
        transform.SetParent(targetPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    [PunRPC]
    public void RPC_Lift(int killerViewId)
    {
        if (killerViewId == -1)
        {
            transform.SetParent(null);
            _controller.SetPhysical(true);
            if (TryGetComponent(out PhotonTransformView ptv)) ptv.enabled = true;
        }
        else
        {
            PhotonView killerPv = PhotonView.Find(killerViewId);
            if (killerPv != null)
            {
                var killerCtrl = killerPv.GetComponent<KillerController>();
                Transform liftPoint = killerCtrl.GetLiftPoint();
                transform.SetParent(liftPoint);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                _controller.SetPhysical(false);
                if (TryGetComponent(out PhotonTransformView ptv)) ptv.enabled = false;
            }
        }
    }

    private void ApplyInteractableFlags(SurvivorState state)
    {
        IsSurvivorInteractable = (state == SurvivorState.Hurt || state == SurvivorState.Dying || state == SurvivorState.Hang);
        IsKillerInteractable = (state == SurvivorState.Dying || state == SurvivorState.Lifted);
    }

    private void ApplyStateVisual(SurvivorState state)
    {
        if (_animator != null)
        {
            _animator.SetBool("IsHurt", state == SurvivorState.Hurt);
            _animator.SetBool("IsDying", state == SurvivorState.Dying);
            _animator.SetBool("IsLifted", state == SurvivorState.Lifted);
            _animator.SetBool("IsHang", state == SurvivorState.Hang);
            _animator.SetBool("IsDead", state == SurvivorState.Dead);
        }

        if (_controller == null) return;

        switch (state)
        {
            case SurvivorState.None:
            case SurvivorState.Hurt:
                _controller.SetMoveSpeed(_controller.WalkSpeed);
                break;
            case SurvivorState.Dying:
                _controller.SetMoveSpeed(_controller.DyingSpeed);
                break;
            case SurvivorState.Lifted:
            case SurvivorState.Hang:
            case SurvivorState.Dead:
                _controller.SetMoveSpeed(0f);
                break;
        }
    }

    [PunRPC]
    public void RPC_SurvivorInteractStart(int requesterViewId)
    {
        if (!_pv.IsMine) return;
        if (CurrentState == SurvivorState.Hurt || CurrentState == SurvivorState.Dying) _healers.Add(requesterViewId);
        else if (CurrentState == SurvivorState.Hang) _rescuers.Add(requesterViewId);
    }

    [PunRPC]
    public void RPC_SurvivorInteractStop(int requesterViewId)
    {
        if (!_pv.IsMine) return;
        _healers.Remove(requesterViewId);
        _rescuers.Remove(requesterViewId);
    }
    
    private void HealingLogic()
    {
        if (_healers.Count == 0) return;
        _healGauge += (_healers.Count / healTime) * Time.deltaTime;

        if (_healGauge >= 1f)
        {
            _healGauge = 1f;
            RequestStateChange(CurrentState == SurvivorState.Dying ? SurvivorState.Hurt : SurvivorState.None, true);
            
            _pv.RPC(nameof(RPC_ForceStopInteractors), RpcTarget.All);
        }
    }

    
    private void RescueLogic()
    {
        if (_rescuers.Count == 0) return;
        _rescueGauge += (_rescuers.Count / rescueTime) * Time.deltaTime;

        if (_rescueGauge >= 1f)
        {
            _rescueGauge = 1f;
            RequestStateChange(SurvivorState.Hurt, true);
            _pv.RPC(nameof(RPC_FinalizeRescue), RpcTarget.All);
            
            _pv.RPC(nameof(RPC_ForceStopInteractors), RpcTarget.All);
        }
    }

    
    [PunRPC]
    private void RPC_ForceStopInteractors()
    {        
        var localPlayer = InGameManager.Instance.GetCharacterObject();
        if (localPlayer != null)
        {
            var interactManager = localPlayer.GetComponent<SurvivorInteractManager>();
     
            if (interactManager.CurrentTargetViewId == _pv.ViewID)
            {
                localPlayer.GetComponent<SurvivorController>().Interact(false);
            }
        }
    }

    [PunRPC]
    private void RPC_FinalizeRescue()
    {
        Hook hook = GetComponentInParent<Hook>();
        if (hook != null)
        {            
            Vector3 rescuePos = hook.transform.position + (hook.transform.forward * 1.0f);
            transform.position = rescuePos;

            hook.OnRescueComplete();
        }

        transform.SetParent(null);
        
        _controller.SetPhysical(true);
        if (TryGetComponent(out PhotonTransformView ptv))
        {
            ptv.enabled = true;
        }

        Debug.Log("구출 완료 및 물리 복구");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)CurrentState);
            stream.SendNext(IsSitting);
            stream.SendNext(IsInteracting);
            stream.SendNext(_healGauge);
            stream.SendNext(_rescueGauge);
        }
        else
        {
            CurrentState = (SurvivorState)(int)stream.ReceiveNext();
            IsSitting = (bool)stream.ReceiveNext();
            IsInteracting = (bool)stream.ReceiveNext();
            _healGauge = (float)stream.ReceiveNext();
            _rescueGauge = (float)stream.ReceiveNext();
            
            if (InGameUIManager.Instance != null)
            {
                InGameUIManager.Instance.UpdateSurvivorStatus(_pv.ViewID, CurrentState);
            }

            ApplyStateVisual(CurrentState);
        }
    }
}