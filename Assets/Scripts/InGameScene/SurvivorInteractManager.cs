using System.Collections.Generic;
using UnityEngine;

public class SurvivorInteractManager : MonoBehaviour
{
    private Collider _nearest;
    private List<Collider> _overlaps = new List<Collider>();
    private IInteractable _currentInterectable;    

    private void Update()
    {
        _nearest = GetNearestInTrigger();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InteractRange"))
        {
            _overlaps.Add(other);
            Debug.Log("상호작용 대상 발견");
        }        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteractRange"))
        {
            _overlaps.Remove(other);
            Debug.Log("상호작용 대상 제거");
        }            
    }

    public void StartInteract()
    {        
        if (_nearest == null)
        {
            return;
        }
        _currentInterectable = _nearest.GetComponent<IInteractable>();
        _currentInterectable.StartInteract();
    }

    public void StopInteract()
    { 
        if (_currentInterectable == null)
        {
            return;
        }
        _currentInterectable.StopInteract();
        _currentInterectable = null;
    }

    private Collider GetNearestInTrigger()
    {
        Collider nearest = null;
        float minDist = float.MaxValue;
        
        foreach (var col in _overlaps)
        {
            if (col == null)
            {
                continue;
            }

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
