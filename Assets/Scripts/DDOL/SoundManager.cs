using UnityEngine;
using System.Collections.Generic;

public class SoundManager : Singleton<SoundManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _bgmSource;    

    [Header("Audio Clips")]    
    private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    protected override void Awake()
    {
        base.Awake();
        
        if (_bgmSource == null)
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
        }
        LoadAllSounds();
    }

    private void Start()
    {
        PlayBGM("LobbyBGM", 0.3f);
    }

    // 리소스 폴더의 "Sounds" 폴더 내 모든 사운드를 로드하는 매서드
    private void LoadAllSounds()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sounds");
        foreach (var clip in clips)
        {
            if (!_audioClips.ContainsKey(clip.name))
            {
                _audioClips.Add(clip.name, clip);
            }                
        }
    }

    // 메인 배경음악
    public void PlayBGM(string clipName, float volume = 0.5f)
    {
        if (_audioClips.TryGetValue(clipName, out AudioClip clip))
        {
            _bgmSource.clip = clip;
            _bgmSource.volume = volume;
            _bgmSource.Play();
        }
        else
        {
            Debug.Log($"요청한 배경음악 {clipName}이 리소스 폴더에 존재하지 않음");
        }
    }

    // 효과음
    public void PlaySFX(string clipName, float volume = 1f)
    {
        if (_audioClips.TryGetValue(clipName, out AudioClip clip))
        {            
            _bgmSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.Log($"요청한 효과음 {clipName}이 리소스 폴더에 존재하지 않음");
        }
    }
}