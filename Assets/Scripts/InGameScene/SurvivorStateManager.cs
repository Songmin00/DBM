using Photon.Pun;
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

    private void Start()
    {
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
        if (CurrentState == SurvivorState.None)
            RequestStateChange(SurvivorState.Hurt, true);
        else if (CurrentState == SurvivorState.Hurt)
            RequestStateChange(SurvivorState.Dying, true);
    }

    // 상태 변경을 네트워크 전체에 요청하는 메서드
    public void RequestStateChange(SurvivorState next, bool resetProgress)
    {
        _pv.RPC(nameof(RPC_SyncState), RpcTarget.All, (int)next, resetProgress);
    }

    [PunRPC]
    private void RPC_SyncState(int nextState, bool resetProgress)
    {
        SurvivorState next = (SurvivorState)nextState;
        if (CurrentState == next) return;

        CurrentState = next;

        if (resetProgress)
        {
            _healers.Clear();
            _rescuers.Clear();
            _healGauge = 0f;
            _rescueGauge = 0f;
        }

        ApplyStateVisual(next);
        ApplyInteractableFlags(next);
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
        if (CurrentState == SurvivorState.Dying) return true;
        return true;
    }

    public void StartSurvivorInteract() => SetInteracting(true);
    public void StopSurvivorInteract() => SetInteracting(false);

    public void StartKillerInteract(KillerController killer)
    {
        if (killer == null) return;
        PhotonView killerPv = killer.GetComponent<PhotonView>();
        if (killerPv == null || _pv == null) return;

        // 생존자 오너에게 알림
        _pv.RPC(nameof(RPC_KillerInteract), _pv.Owner, killerPv.ViewID);
    }

    public void StopKillerInteract() { }

    [PunRPC]
    private void RPC_KillerInteract(int killerViewId)
    {
        if (_pv == null || !_pv.IsMine) return;

        if (CurrentState == SurvivorState.Dying)
        {
            // 1. 먼저 상태를 '들림'으로 모든 클라이언트 동기화
            RequestStateChange(SurvivorState.Lifted, true);

            // 2. 모든 클라이언트에서 킬러의 어깨에 붙도록 실행
            _pv.RPC(nameof(RPC_Lift), RpcTarget.All, killerViewId);
        }
        else if (CurrentState == SurvivorState.Lifted)
        {
            RequestStateChange(SurvivorState.Hang, true);
        }
    }
    // ... 기존 코드 유지 ...

    [PunRPC]
    public void RPC_AttachToHook(int hookPointViewId)
    {
        PhotonView pointPv = PhotonView.Find(hookPointViewId);
        if (pointPv != null)
        {
            // 킬러의 어깨에서 갈고리로 부모 변경
            transform.SetParent(pointPv.transform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            _controller.SetPhysical(false);
            if (TryGetComponent(out PhotonTransformView ptv)) ptv.enabled = false;
        }
    }

    
    [PunRPC]
    public void RPC_Lift(int killerViewId)
    {
        if (killerViewId == -1) // 해제 (내려놓기 또는 구출됨)
        {
            transform.SetParent(null);
            _controller.SetPhysical(true);
            if (TryGetComponent(out PhotonTransformView ptv)) ptv.enabled = true;
        }
        else // 킬러가 들기
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

    // ... 나머지 로직 유지 ...
    private void ApplyInteractableFlags(SurvivorState state)
    {
        IsSurvivorInteractable = (state == SurvivorState.Hurt || state == SurvivorState.Hang);
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
        }

        if (_controller == null) return;

        switch (state)
        {
            case SurvivorState.None:
            case SurvivorState.Hurt:
                _controller.SetMoveSpeed(4f);
                break;
            case SurvivorState.Dying:
                _controller.SetMoveSpeed(1.5f);
                break;
            case SurvivorState.Lifted:
            case SurvivorState.Hang:
                // 이동 속도를 0으로 만들어 혹시 모를 입력을 차단
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
        if (_healGauge >= 1f) RequestStateChange(CurrentState == SurvivorState.Dying ? SurvivorState.Hurt : SurvivorState.None, true);
    }

    private void RescueLogic()
    {
        if (_rescuers.Count == 0) return;
        _rescueGauge += (_rescuers.Count / rescueTime) * Time.deltaTime;
        if (_rescueGauge >= 1f)
        {
            RequestStateChange(SurvivorState.Hurt, true);
            _pv.RPC(nameof(RPC_Lift), RpcTarget.All, -1);
        }
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
            // 중요 상태 변경은 RPC가 처리하므로, 여기서는 값 수신 후 비주얼만 갱신
            CurrentState = (SurvivorState)(int)stream.ReceiveNext();
            IsSitting = (bool)stream.ReceiveNext();
            IsInteracting = (bool)stream.ReceiveNext();
            _healGauge = (float)stream.ReceiveNext();
            _rescueGauge = (float)stream.ReceiveNext();
            ApplyStateVisual(CurrentState);
        }
    }
}