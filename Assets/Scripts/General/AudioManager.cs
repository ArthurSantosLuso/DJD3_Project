using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip sceneMusic;
    [Range(0, 1)][SerializeField] private float musicVolume = 0.09f;
    [Range(0, 1)][SerializeField] private float sfxVolume = 1f;

    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
                FindFirstObjectByType<AudioManager>().Init();

            return _instance;
        }
    }

    private List<AudioSource> audioSources;

    void Awake()
    {
        if (_instance == null)
            Init();
        else if (_instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {            
        PlayMusic(sceneMusic);
    }

    private void Init()
    {
        _instance = this;
        //DontDestroyOnLoad(gameObject);  

        audioSources = new List<AudioSource>();
    }

    public void PlaySound(AudioClip audio, float pitch = 1f)
    {
        AudioSource audioSource;
        CheckForFreeAudioSource(out audioSource);
        if (audioSource == null)
            audioSource = CreateNewAudioSource();

        audioSource.pitch = pitch;
        audioSource.PlayOneShot(audio, sfxVolume);
    }

    public void PlayMusic(AudioClip audio)
    {
        AudioSource audioSource;
        CheckForFreeAudioSource(out audioSource);

        if (audioSource == null)
        {
            audioSource = CreateNewAudioSource();
        }
        audioSource.volume = musicVolume;
        audioSource.loop = true;
        audioSource.clip = audio;
        audioSource.Play();
    }

    public void CheckForFreeAudioSource(out AudioSource source)
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (!audioSource.isPlaying)
            {
                source = audioSource;
                return;
            }
        }

        source = null;
    }

    public AudioSource CreateNewAudioSource()
    {
        AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
        audioSources.Add(newAudioSource);
        return newAudioSource;
    }

    public AudioSource PlayLoopingSound(AudioClip audio)
    {
        AudioSource audioSource = CreateNewAudioSource();
        audioSource.clip = audio;
        audioSource.loop = true;
        audioSource.volume = sfxVolume;
        audioSource.Play();
        return audioSource;
    }
}
