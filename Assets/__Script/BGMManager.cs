using UnityEngine;
using UnityEngine.UI;

public class BGMManager : MonoBehaviour
{
    static BGMManager instance;
    static AudioSource audioSource;
    public AudioClip clip;

    void Awake()
    {
        if (!instance)
        {
            instance = this;
            audioSource = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (clip)
        {
            PlayBGM(false, clip);
        }
    }

    public static void PlayBGM(bool reset, AudioClip clip = null)
    {
        if (clip)
        {
            audioSource.clip = clip;
        }
        
        if (audioSource.clip)
        {
            if (reset)
            {
                audioSource.Stop();
            }
            audioSource.Play();
        }    
    }

    public static void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public static void PauseBGM()
    {
        audioSource.Pause();
    }
}
