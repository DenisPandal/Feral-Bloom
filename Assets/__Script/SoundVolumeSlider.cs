using UnityEngine;
using UnityEngine.UI;

public class SoundVolumeSlider : MonoBehaviour
{
    Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        float volume = PlayerPrefs.GetFloat("SoundVolume", 0.3f);
        slider.value = volume;
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value)
    {
        PlayerPrefs.SetFloat("SoundVolume", value);
    }

    void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }
}
