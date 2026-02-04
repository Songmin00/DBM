using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Collections.Generic;

public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager Instance;

    [Header("Generator UI")]
    [SerializeField] private TextMeshProUGUI _generatorCountText;

    [Header("Survivor Status UI")]
    [SerializeField] private GameObject _survivorSlotPrefab;
    [SerializeField] private Transform _survivorStatusParent;

    
    private Dictionary<int, SurvivorStatusSlot> _statusSlots = new Dictionary<int, SurvivorStatusSlot>();

    private void Awake()
    {
        Instance = this;
    }    
    
    public void UpdateGeneratorUI(int remaining)
    {
        if (remaining <= 0)
        {
            _generatorCountText.text = "<color=red>문 열림!</color>";
        }
        else
        {
            _generatorCountText.text = $"남은 발전기 : {remaining}";
        }
    }
    
    public void RegisterSurvivor(int viewId, string playerName)
    {
        if (_statusSlots.ContainsKey(viewId)) return;

        GameObject go = Instantiate(_survivorSlotPrefab, _survivorStatusParent);
        SurvivorStatusSlot slot = go.GetComponent<SurvivorStatusSlot>();
        slot.Init(playerName);
        _statusSlots.Add(viewId, slot);
    }
    
    public void UpdateSurvivorStatus(int viewId, SurvivorState state)
    {
        if (_statusSlots.TryGetValue(viewId, out var slot))
        {
            slot.UpdateState(state);
        }
    }
}