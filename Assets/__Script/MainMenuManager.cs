using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "FeralBloom";

    private UIDocument uiDocument;
    private Button startBtn;
    private Button quitBtn;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            var root = uiDocument.rootVisualElement;

            startBtn = root.Q<Button>("StartButton");
            if (startBtn != null) startBtn.clicked += StartGame;

            quitBtn = root.Q<Button>("QuitButton");
            if (quitBtn != null) quitBtn.clicked += QuitGame;
        }
    }

    void OnDisable()
    {
        if (startBtn != null) startBtn.clicked -= StartGame;
        if (quitBtn != null) quitBtn.clicked -= QuitGame;
    }

    void StartGame()
    {
        Time.timeScale = 1f;
        GameManager.isGameOn = true;
        if (GameManager.health <= 0)
        {
            GameManager.health = 3;
        }
        SceneManager.LoadScene(targetSceneName);
    }

    void QuitGame()
    {
        Debug.Log("Quit Game!");
        Application.Quit();
    }
}
