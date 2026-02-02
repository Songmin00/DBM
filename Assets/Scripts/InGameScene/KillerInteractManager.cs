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
        if (other == Nearest)
        {
            StopInteract();
        }
        Debug.Log("상호작용 대상 제거");
    }

    public void StartInteract()
    {
        if (Nearest == null) return;

        // 디버그 (너가 넣은 그대로 유지)
        if (Nearest.GetComponentInParent<SurvivorStateManager>() != null)
            Debug.Log($"State: {Nearest.GetComponentInParent<SurvivorStateManager>().CurrentState}");

        var killerInteractable = Nearest.GetComponentInParent<IKillerInteractable>();
        if (killerInteractable != null)
            Debug.Log($"IsKillerInteractable: {killerInteractable.IsKillerInteractable}");

        _currentInterectable = Nearest.GetComponentInParent<IKillerInteractable>();

        if (_currentInterectable == null) return;
        if (!_currentInterectable.IsKillerInteractable) return;

        Transform target = Nearest.transform.root;
        _controller.LookAtTarget(target);

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
            if (col == null)
            {
                _overlaps.RemoveAt(i);
                continue;
            }

            var interactable = col.GetComponentInParent<IKillerInteractable>();
            if (interactable == null) continue;
            if (!interactable.IsKillerInteractable) continue;

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
