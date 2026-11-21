using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "Breakout";

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject scoresPanel;

    [Header("Widgets")]
    public Text scoresText;
    public Button startButton;
    public Button backButton;

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // 브라우저에서는 종료가 의미 없으므로 QUIT 버튼 숨김
        var quit = mainPanel ? mainPanel.transform.Find("QuitButton") : null;
        if (quit) quit.gameObject.SetActive(false);
#endif
        ShowMain();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame && scoresPanel.activeSelf)
            ShowMain();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowScores()
    {
        var scores = HighScores.Load();
        var sb = new StringBuilder();
        for (int i = 0; i < HighScores.MaxEntries; i++)
        {
            string value = i < scores.Count ? scores[i].ToString() : "---";
            sb.AppendLine($"{i + 1}.   {value}");
        }
        scoresText.text = sb.ToString();

        mainPanel.SetActive(false);
        scoresPanel.SetActive(true);
        Select(backButton);
    }

    public void ShowMain()
    {
        scoresPanel.SetActive(false);
        mainPanel.SetActive(true);
        Select(startButton);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    static void Select(Button button)
    {
        if (button && EventSystem.current)
            EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
}
