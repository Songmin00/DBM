using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class KillerInteractManager : MonoBehaviour
{
    public Collider Nearest { get; private set; }

    private readonly List<Collider> _overlaps = new List<Collider>();
    private IKillerInteractable _currentInterectable;
    private KillerController _controller;
    private PhotonView _pv;

    private void Awake()
    {
        _controller = GetComponent<KillerController>();
        _pv = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (_pv == null || !_pv.IsMine) return;
        Nearest = GetNearestInTrigger();
    }

    private void OnTriggerEnter(Collider other)
    {        
        //Debug.Log($"트리거 진입: {other.name} | 태그: {other.tag}");

        if (!other.CompareTag("SurvivorInteractRange") && !other.CompareTag("Hook")) return;

        if (!_overlaps.Contains(other))
        {
            _overlaps.Add(other);
            //Debug.Log($"관리 리스트에 추가됨: {other.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("SurvivorInteractRange") && !other.CompareTag("Hook")) return;
        _overlaps.Remove(other);
        if (other == Nearest)
        {
            StopInteract();
        }
    }
    
    public void StartInteract()
    {
        if (Nearest == null) return;

        _currentInterectable = Nearest.GetComponentInParent<IKillerInteractable>();
        if (_currentInterectable == null || !_currentInterectable.IsKillerInteractable) return;

        
        Vector3 lookPos = Nearest.transform.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        _currentInterectable.StartKillerInteract(_controller);
    }

    public void StopInteract()
    {
        if (_currentInterectable == null) return;
        _currentInterectable.StopKillerInteract();
        _currentInterectable = null;
    }

    private Collider GetNearestInTrigger()
    {
        Collider nearest = null;
        float minDist = float.MaxValue;

        for (int i = _overlaps.Count - 1; i >= 0; i--)
        {
            Collider col = _overlaps[i];
            if (col == null) { _overlaps.RemoveAt(i); continue; }

            var interactable = col.GetComponentInParent<IKillerInteractable>();
            if (interactable == null || !interactable.IsKillerInteractable) continue;

            
            // 만약 생존자를 들고 있다면
            bool isCarrying = _controller.GetLiftPoint().childCount > 0;

            if (isCarrying)
            {
                if (!col.CompareTag("Hook")) continue;
            }
            else //안 들고 있다면
            {                
                if (col.CompareTag("Hook")) continue;
            }
            // -----------------------

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