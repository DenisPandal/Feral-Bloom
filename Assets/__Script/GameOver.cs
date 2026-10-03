using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void GameRestart()
    {
        GameManager.isGameOn = true;
        Time.timeScale = 1.0f;
        GameManager.health = 3;
        GameManager.score = 0;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        BGMManager.PlayBGM(true);
        gameObject.SetActive(false);
    }

    public void GameQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}