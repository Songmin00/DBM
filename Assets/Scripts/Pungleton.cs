using Photon.Pun;
using UnityEngine;

public class Pungleton<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacks
{
    private static T _instance;
    public static T Instance => _instance;

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(this);
        }
    }
}
