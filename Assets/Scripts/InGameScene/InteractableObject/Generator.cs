using UnityEngine;

public class Generator : MonoBehaviour, IInteractable
{
    private float gauge = 80;
    private float _currentGauge = 0;    
    private int _fixingSurvivors = 0;
    private bool  _isFixed = false;

    private void Update()
    {
        if (_isFixed)
        {
            return;
        }
        if (_fixingSurvivors <= 0)
        {
            return;
        }

        FixingRogic();
    }

    public void StartInteract()
    {        
        _fixingSurvivors++;
    }
    public void StopInteract()
    {        
        _fixingSurvivors = Mathf.Max(0, _fixingSurvivors - 1);             
    }

    private void FixingRogic()
    {
        _currentGauge += (1 * Time.deltaTime) * (1 + ((_fixingSurvivors - 1) * 0.5f));
        Debug.Log("ÁøÇàµµ"+_currentGauge);
        if (_currentGauge >= gauge)
        {
            _currentGauge = gauge;
            GetFixed();
        }
    }

    private void GetFixed()
    {
        _isFixed = true;
        Debug.Log("Fixed!!");
    }
}
