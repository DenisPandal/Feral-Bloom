using UnityEngine;
using UnityEngine.UI;

public class SoundFXManager : MonoBehaviour
{
    static SoundFXManager instance;
    static AudioSource audioSource;
    static SoundFXLibrary soundSFXLibrary;
    public Slider volumeSlider;
    
    void Awake()
    {
        if (!instance)
        {
            instance = this;
            audioSource = GetComponent<AudioSource>();
            soundSFXLibrary = GetComponent<SoundFXLibrary>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    public static void Play(string clipname)
    {
        AudioClip clip = soundSFXLibrary.GetRandomClip(clipname);
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public static void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }
}
