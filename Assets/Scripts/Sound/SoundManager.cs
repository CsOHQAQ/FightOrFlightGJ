using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;


    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
    }

    public List<Sound> sounds;

    private Dictionary<string, AudioClip> soundDictionary;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = gameObject.AddComponent<AudioSource>();
            soundDictionary = new Dictionary<string, AudioClip>();

            foreach (var sound in sounds)
            {
                soundDictionary[sound.name] = sound.clip;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // PlayBackgroundMusic("sonic");
    }

    public void PlaySound(string name, float volume)
    {
        if (soundDictionary.TryGetValue(name, out var clip))
        {

            audioSource.clip = clip;
            audioSource.volume = Mathf.Clamp(volume, 0.0f, 1.0f);
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"Sound: {name} not found");
        }
    }

    public void PlayBackgroundMusic(string name)
    {
        if (soundDictionary.TryGetValue(name, out var clip))
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"Background Music: {name} not found");
        }
    }

    public Dictionary<string, AudioClip> GetSoundDict()
    {
        return soundDictionary;
    }

    public void StopBackgroundMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
