using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;

public class Options : MonoBehaviour
{

    [SerializeField] RectTransform optionsPanel;
    [SerializeField] float OffPosY, OnPosY;
    [SerializeField] float tweenDuration;
    [SerializeField] CanvasGroup screenShadow;
    [SerializeField] Slider soundSlider;
    [SerializeField] Slider musicSlider;
    float soundVolume;
    float musicVolume;

    public void OptionsOpen()
    {
        soundVolume = soundSlider.value;
        musicVolume = musicSlider.value;
        Time.timeScale = 0;
        BGMManager.PauseBGM();
        OptionPanelIntro();
    }

    public async void OptionsClose()
    {
        await OptionPanelOutro();
        BGMManager.PlayBGM(false);
        Time.timeScale = 1;
    }

    public void Ok()
    {
        OptionsClose();
    }

    public void Cancel()
    {
        soundSlider.value = soundVolume;
        musicSlider.value = musicVolume;
        OptionsClose();
    }

    void OptionPanelIntro()
    {
        screenShadow.DOFade(1, tweenDuration).SetUpdate(true);
        optionsPanel.DOAnchorPosY(OnPosY, tweenDuration).SetUpdate(true);
    }

    async Task OptionPanelOutro()
    {
        screenShadow.DOFade(0, tweenDuration).SetUpdate(true);
        await optionsPanel.DOAnchorPosY(OffPosY, tweenDuration).SetUpdate(true).AsyncWaitForCompletion();
    }
}
