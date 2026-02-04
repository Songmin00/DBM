using UnityEngine;
using TMPro;

public class SurvivorStatusSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _stateText;

    public void Init(string nickname)
    {
        _nameText.text = nickname;
        UpdateState(SurvivorState.None); // 기본 상태
    }

    public void UpdateState(SurvivorState state)
    {
        switch (state)
        {
            case SurvivorState.None: _stateText.text = "건강"; _stateText.color = Color.white; break;
            case SurvivorState.Hurt: _stateText.text = "부상"; _stateText.color = Color.yellow; break;
            case SurvivorState.Dying: _stateText.text = "빈사"; _stateText.color = Color.red; break;
            case SurvivorState.Hang: _stateText.text = "매달림"; _stateText.color = Color.magenta; break;
            case SurvivorState.Dead: _stateText.text = "사망"; _stateText.color = Color.gray; break;
        }
    }
}