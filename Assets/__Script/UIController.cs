using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject gameOver;
    [SerializeField] Options options;
    bool isPanelOptionsOn;
    bool isPanelGameOverOn;

    void Start()
    {
        PlayerHealth.OnDead += GameOver;
    }
    void OnDisable()
    {
        PlayerHealth.OnDead -= GameOver;
    }

    void GameOver()
    {
        GameManager.isGameOn = false;
        BGMManager.PauseBGM();
        Time.timeScale = 0.0f;
        gameOver.SetActive(true);
    }

    public void ESCPressed(InputAction.CallbackContext context)
    {
        if (!context.performed || !GameManager.isGameOn)
        {
            return;
        }
        
        if (!isPanelOptionsOn)
        {
            isPanelOptionsOn = true;
            options.OptionsOpen();
        }
        else
        {
            options.OptionsClose();
            isPanelOptionsOn = false;
        }
    }
}
