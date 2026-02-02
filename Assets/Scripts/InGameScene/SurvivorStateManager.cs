using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public enum SurvivorState
{
    None, Hurt, Dying, Lifted, Hang, Survive, Dead
}

public class SurvivorStateManager : MonoBehaviour, ISurvivorInteractable, IKillerInteractable, IPunObservable
{
    private Animator _animator;
    private SurvivorController _controller;
    private PhotonView _pv;

    public SurvivorState CurrentState { get; private set; } = SurvivorState.None;

    public bool IsSitting { get; private set; }
    public bool IsInteracting { get; private set; }

    // 인터페이스 플래그
    public bool IsSurvivorInteractable { get; set; } = false;
    public bool IsKillerInteractable { get; set; } = false;

    // Healing
    [SerializeField] private float healTime = 16f;
    private float _healGauge = 0f;
    private readonly HashSet<int> _healers = new HashSet<int>();

    // Rescue
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
        // 시작 시 상태 적용 (시각/플래그 모두)
        ApplyStateVisual(CurrentState);
        ApplyInteractableFlags(CurrentState);

        // 앉기/인터랙트 기본값도 명시
        if (_animator != null)
        {
            _animator.SetBool("IsSitting", IsSitting);
            _animator.SetBool("IsInteracting", IsInteracting);
        }
    }

    private void Update()
    {
        // 게이지 진행은 오너만
        if (_pv == null || !_pv.IsMine) return;

        if (CurrentState == SurvivorState.Hurt || CurrentState == SurvivorState.Dying)
            HealingLogic();

        if (CurrentState == SurvivorState.Hang)
            RescueLogic();
    }

    // ================= External =================

    public void OnHit()
    {
        if (CurrentState == SurvivorState.None)
            ChangeState(SurvivorState.Hurt, resetProgress: true);
        else if (CurrentState == SurvivorState.Hurt)
            ChangeState(SurvivorState.Dying, resetProgress: true);
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
        return true;
    }

    // ================= ISurvivorInteractable =================
    // ※ 발전기/기타가 직접 호출할 수도 있어서 구현은 유지.
    //    "생존자 치료"는 InteractManager가 RPC_SurvivorInteractStart(viewId)로 호출하는 방식이 메인.

    public void StartSurvivorInteract()
    {
        // 누가 상호작용했는지(viewId)가 없으므로
        // 치료 카운트에는 넣지 않고 애니만 켠다(안전장치).
        SetInteracting(true);
    }

    public void StopSurvivorInteract()
    {
        SetInteracting(false);
    }

    // ================= IKillerInteractable =================

    public void StartKillerInteract(KillerController killer)
    {
        if (killer == null) return;
        PhotonView killerPv = killer.GetComponent<PhotonView>();
        if (killerPv == null) return;
        if (_pv == null) return;

        // 생존자 오너에게 상태 전환 요청
        _pv.RPC(nameof(RPC_KillerInteract), _pv.Owner, killerPv.ViewID);
    }

    public void StopKillerInteract()
    {
        // 필요 시 확장 (내려놓기 등)
    }

    [PunRPC]
    private void RPC_KillerInteract(int killerViewId)
    {
        if (_pv == null || !_pv.IsMine) return;

        if (CurrentState == SurvivorState.Dying)
        {
            ChangeState(SurvivorState.Lifted, resetProgress: true);
        }
        else if (CurrentState == SurvivorState.Lifted)
        {
            ChangeState(SurvivorState.Hang, resetProgress: true);
        }
    }

    // ================= FSM =================

    private void ChangeState(SurvivorState next, bool resetProgress)
    {
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

    private void ApplyInteractableFlags(SurvivorState state)
    {
        IsSurvivorInteractable = false;
        IsKillerInteractable = false;

        switch (state)
        {
            case SurvivorState.Hurt:
                IsSurvivorInteractable = true; // 치료 가능
                break;

            case SurvivorState.Dying:
                IsKillerInteractable = true; // 들쳐업기 가능
                break;

            case SurvivorState.Hang:
                IsSurvivorInteractable = true; // 구조 가능
                break;
        }
    }

    private void ApplyStateVisual(SurvivorState state)
    {
        if (_animator != null)
        {
            _animator.SetBool("IsHurt", false);
            _animator.SetBool("IsDying", false);
            _animator.SetBool("IsLifted", false);
            _animator.SetBool("IsHang", false);
        }

        if (_controller == null) return;

        switch (state)
        {
            case SurvivorState.None:
                _controller.SetMoveSpeed(4f);
                _controller.SetPhysical(true);
                break;

            case SurvivorState.Hurt:
                _controller.SetMoveSpeed(4f);
                _controller.SetPhysical(true);
                if (_animator != null) _animator.SetBool("IsHurt", true);
                break;

            case SurvivorState.Dying:
                _controller.SetMoveSpeed(1.5f);
                _controller.SetPhysical(true);
                if (_animator != null) _animator.SetBool("IsDying", true);
                break;

            case SurvivorState.Lifted:
                _controller.SetPhysical(false);
                if (_animator != null) _animator.SetBool("IsLifted", true);
                break;

            case SurvivorState.Hang:
                _controller.SetPhysical(false);
                if (_animator != null) _animator.SetBool("IsHang", true);
                break;
        }

        // sitting/interacting는 상태 전환에서 끄지 않는다(네 요구: 공존 가능)
        if (_animator != null)
        {
            _animator.SetBool("IsSitting", IsSitting);
            _animator.SetBool("IsInteracting", IsInteracting);
        }
    }

    // ================= RPC (치료/구조 핵심) =================

    [PunRPC]
    public void RPC_SurvivorInteractStart(int requesterViewId)
    {
        if (_pv == null || !_pv.IsMine) return;

        if (CurrentState == SurvivorState.Hurt || CurrentState == SurvivorState.Dying)
        {
            _healers.Add(requesterViewId);
        }
        else if (CurrentState == SurvivorState.Hang)
        {
            _rescuers.Add(requesterViewId);
        }
    }

    [PunRPC]
    public void RPC_SurvivorInteractStop(int requesterViewId)
    {
        if (_pv == null || !_pv.IsMine) return;

        _healers.Remove(requesterViewId);
        _rescuers.Remove(requesterViewId);
    }

    // ================= Logic =================

    private void HealingLogic()
    {
        if (_healers.Count == 0) return;

        float speed = _healers.Count * (1f / healTime);
        _healGauge += speed * Time.deltaTime;
        _healGauge = Mathf.Clamp01(_healGauge);

        if (_healGauge >= 1f)
        {
            if (CurrentState == SurvivorState.Dying)
                ChangeState(SurvivorState.Hurt, resetProgress: true);
            else if (CurrentState == SurvivorState.Hurt)
                ChangeState(SurvivorState.None, resetProgress: true);
        }
    }

    private void RescueLogic()
    {
        if (_rescuers.Count == 0) return;

        float speed = _rescuers.Count * (1f / rescueTime);
        _rescueGauge += speed * Time.deltaTime;
        _rescueGauge = Mathf.Clamp01(_rescueGauge);

        if (_rescueGauge >= 1f)
        {
            ChangeState(SurvivorState.Hurt, resetProgress: true);
        }
    }

    // ================= Sync =================

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)CurrentState);
            stream.SendNext(IsSitting);
            stream.SendNext(IsInteracting);
            stream.SendNext(_healGauge);
            stream.SendNext(_rescueGauge);
            stream.SendNext(IsSurvivorInteractable);
            stream.SendNext(IsKillerInteractable);
        }
        else
        {
            CurrentState = (SurvivorState)(int)stream.ReceiveNext();
            IsSitting = (bool)stream.ReceiveNext();
            IsInteracting = (bool)stream.ReceiveNext();
            _healGauge = (float)stream.ReceiveNext();
            _rescueGauge = (float)stream.ReceiveNext();
            IsSurvivorInteractable = (bool)stream.ReceiveNext();
            IsKillerInteractable = (bool)stream.ReceiveNext();

            // 원격은 "시각/플래그"만 반영
            ApplyStateVisual(CurrentState);

            if (_animator != null)
            {
                _animator.SetBool("IsSitting", IsSitting);
                _animator.SetBool("IsInteracting", IsInteracting);
            }
        }
    }
}
