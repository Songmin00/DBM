using UnityEngine;

public enum SurvivorState
{
    None, Hurt, Dying, Lifted, Hang, Survive, Dead
}

public class SurvivorStateManager : MonoBehaviour, IInteractable
{
    public bool IsInteractable { get; set; } = false;

    // ---------- Healing ----------
    private float healTime = 16f;
    private float _healGauge = 0f;
    private int _healingSurvivors = 0;

    // ---------- Rescue ----------
    private float rescueTime = 1.5f;
    private float _rescueGauge = 0f;
    private bool _isRescuing = false;

    public SurvivorState CurrentState { get; set; } = SurvivorState.None;

    private void Update()
    {
        if (CurrentState == SurvivorState.None)
        {
            return;
        }
        if (CurrentState == SurvivorState.Hurt)
        {
            HealingLogic();
        }

        if (CurrentState == SurvivorState.Hang)
        {
            RescueLogic();
        }
    }

    // ================== Healing ==================

    public void StartInteract()
    {
        if (CurrentState == SurvivorState.Hurt)
        {
            _healingSurvivors++;
        }
        else if (CurrentState == SurvivorState.Hang)
        {
            _isRescuing = true;
        }
    }

    public void StopInteract()
    {
        if (CurrentState == SurvivorState.Hurt)
        {
            _healingSurvivors = Mathf.Max(0, _healingSurvivors - 1);
        }
        else if (CurrentState == SurvivorState.Hang)
        {
            _isRescuing = false;
        }
    }

    private void HealingLogic()
    {
        if (_healingSurvivors <= 0) return;

        float speed = _healingSurvivors * (1f / healTime);

        _healGauge += speed * Time.deltaTime;
        _healGauge = Mathf.Clamp01(_healGauge);

        Debug.Log($"치료 진행도: {_healGauge * 100f}%");

        if (_healGauge >= 1f)
        {
            CompleteHeal();
        }
    }

    private void CompleteHeal()
    {
        _healingSurvivors = 0;
        _healGauge = 1f;
        CurrentState = SurvivorState.None;

        Debug.Log("치료 완료!");
    }

    // ================== Rescue ==================

    private void RescueLogic()
    {
        if (!_isRescuing) return;

        float speed = 1f / rescueTime;

        _rescueGauge += speed * Time.deltaTime;
        _rescueGauge = Mathf.Clamp01(_rescueGauge);

        Debug.Log($"구출 진행도: {_rescueGauge * 100f}%");

        if (_rescueGauge >= 1f)
        {
            CompleteRescue();
        }
    }

    private void CompleteRescue()
    {
        _isRescuing = false;
        _rescueGauge = 1f;
        CurrentState = SurvivorState.None;

        Debug.Log("구출 완료!");
    }
}
