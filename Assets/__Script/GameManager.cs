using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    public static bool isGameOn = true;
    public static int levelCurrent = 0;
    public static int levelMax = 2;
    public static int healthMax = 3;
    public static int health = 3;
    public static int score = 0;
    public static bool levelProgressTrigger = false;
    bool levelLoadedTrigger = true;
    public Image screenShadow;
    public static UnityAction OnScoreChanged;

    void Awake()
    {

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        //need to improve
        if (levelProgressTrigger)
        {
            StartCoroutine(WaitAndLevelLoadNext(1f));
            levelProgressTrigger = false;
        }
        //need to improve
        if (levelLoadedTrigger)
        {
            screenShadow.rectTransform.parent.gameObject.SetActive(false);
            levelLoadedTrigger = false;
        }
    }

    IEnumerator WaitAndLevelLoadNext(float delay)
    {
        float a;

        screenShadow.rectTransform.parent.gameObject.SetActive(true);
        for (float t = 0f; t < delay; t += Time.deltaTime)
        {
            a = Mathf.Lerp(0f, 1f, t / delay);
            screenShadowSetAlpha(a);
            yield return null;
        }
        LevelLoadNext();
        levelLoadedTrigger = true;
    }

    void screenShadowSetAlpha(float alpha)
    {
        Color color = screenShadow.color;

        color.a = alpha;
        screenShadow.color = color;
    }

    public static void ScoreAdd(int scoreToAdd)
    {
        score += scoreToAdd;
        OnScoreChanged?.Invoke();
    }

    public static void LevelLoadNext()
    {
        if (++levelCurrent > levelMax)
        {
            levelCurrent = 0;
        }
        SceneManager.LoadScene(levelCurrent);
    }

    public void LevelLoad(int level)
    {

    }    
}
