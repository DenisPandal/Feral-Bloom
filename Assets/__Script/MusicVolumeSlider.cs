using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MusicVolumeSlider : MonoBehaviour
{
    Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        float volume = PlayerPrefs.GetFloat("MusicVolume", 0.1f);
        slider.value = volume;
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }
}
