using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [System.Serializable]
    public class AudioSetting
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 0.6f;
    }

    [SerializeField] private AudioSetting[] backgroundMusics;
    [SerializeField] private AudioSetting[] soundEffects;

    private AudioSource bgAudioSource;
    private AudioSource effectAudioSource;

    [SerializeField] private string resourceDir = "Music";

    private Dictionary<string, AudioSetting> bgDictionary;
    private Dictionary<string, AudioSetting> effectDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            InitializeDictionaries();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        bgAudioSource = gameObject.AddComponent<AudioSource>();
        bgAudioSource.loop = true;
        bgAudioSource.playOnAwake = false;
        bgAudioSource.volume = 0.5f;

        effectAudioSource = gameObject.AddComponent<AudioSource>();
    }

    private void InitializeDictionaries()
    {
        bgDictionary = new Dictionary<string, AudioSetting>();
        effectDictionary = new Dictionary<string, AudioSetting>();

        foreach (var bg in backgroundMusics)
        {
            bgDictionary[bg.name] = bg;
        }

        foreach (var effect in soundEffects)
        {
            effectDictionary[effect.name] = effect;
        }
    }

    public void PlayBackgroundMusic(string name)
    {
        if (bgAudioSource.clip != null && bgAudioSource.clip.name == name)
        {
            return; // 已经在播放该背景音乐
        }

        if (bgDictionary.TryGetValue(name, out AudioSetting setting))
        {
            bgAudioSource.clip = setting.clip;
            bgAudioSource.volume = setting.volume;
            bgAudioSource.Play();
        }
        else
        {
            LoadAndPlayAudio(name, true);
        }
    }

    public void PlayBackgroundMusic(string[] names)
    {
        PlayRandomBackgroundMusic(names);
    }

    public void PlaySoundEffect(string name)
    {
        if (effectDictionary.TryGetValue(name, out AudioSetting setting))
        {
            effectAudioSource.PlayOneShot(setting.clip, setting.volume);
        }
        else
        {
            LoadAndPlayAudio(name, false);
        }
    }

    public void PlaySoundEffect(string[] names)
    {
        PlayRandomSoundEffect(names);
    }
    private void LoadAndPlayAudio(string name, bool isBackgroundMusic)
    {
        string path = $"{resourceDir}/{name}";
        AudioClip clip = Resources.Load<AudioClip>(path);

        if (clip != null)
        {
            AudioSetting newSetting = new AudioSetting { name = name, clip = clip, volume = 0.5f };

            if (isBackgroundMusic)
            {
                bgDictionary[name] = newSetting;
                PlayBackgroundMusic(name);
            }
            else
            {
                effectDictionary[name] = newSetting;
                PlaySoundEffect(name);
            }
        }
        else
        {
            Debug.LogWarning($"Audio clip not found: {path}");
        }
    }

    public void StopBackgroundMusic()
    {
        bgAudioSource.Stop();
    }

    public void SetBackgroundMusicVolume(float volume)
    {
        bgAudioSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSoundEffectVolume(float volume)
    {
        effectAudioSource.volume = Mathf.Clamp01(volume);
    }

    // 新增方法：从数组中随机选择一个背景音乐播放
    public void PlayRandomBackgroundMusic(string[] names)
    {
        if (names == null || names.Length == 0)
        {
            Debug.LogWarning("No background music names provided.");
            return;
        }

        string randomName = names[Random.Range(0, names.Length)];
        PlayBackgroundMusic(randomName);
    }

    // 新增方法：从数组中随机选择一个音效播放
    public void PlayRandomSoundEffect(string[] names)
    {
        if (names == null || names.Length == 0)
        {
            Debug.LogWarning("No sound effect names provided.");
            return;
        }

        string randomName = names[Random.Range(0, names.Length)];
        PlaySoundEffect(randomName);
    }
}