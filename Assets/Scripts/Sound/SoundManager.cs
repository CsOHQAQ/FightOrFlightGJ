using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void PlaySound(string name)
    {
        if (soundDictionary.TryGetValue(name, out var clip))
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"Sound: {name} not found");
        }
    }
}
