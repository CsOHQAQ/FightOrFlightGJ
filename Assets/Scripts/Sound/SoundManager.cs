using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    // Sound Dictionary
    private SoundManager soundManager;
    private List<string> matchingKeys;


    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
    }

    public List<Sound> sounds;

    private Dictionary<string, AudioClip> soundDictionary;
    private AudioSource audioSource;
    private AudioSource audioSourceMusic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSourceMusic = gameObject.AddComponent<AudioSource>();
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
        soundManager = SoundManager.Instance;
        matchingKeys = soundManager.GetSoundDict().Keys.Where(k => k.StartsWith("Background")).ToList();


        int randomIndex = Random.Range(0, matchingKeys.Count - 1);
        string randomKey = matchingKeys[randomIndex];

        PlayBackgroundMusic(randomKey);
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
            audioSourceMusic.clip = clip;
            audioSourceMusic.loop = true;
            audioSourceMusic.volume = 0.25f;
            audioSourceMusic.Play();
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
