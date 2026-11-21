using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public Ball ball;
    public Text scoreText;
    public Text livesText;
    public Text messageText;
    public Fireworks fireworks;

    [Header("Rules")]
    public int startLives = 3;
    public string menuSceneName = "Menu";

    public int Score { get; private set; }
    public int Lives { get; private set; }
    public bool IsGameOver { get; private set; }

    int bricksRemaining;

    void Awake()
    {
        Instance = this;
        Lives = startLives;
        Score = 0;
    }

    void Start()
    {
        RefreshUI();
        ShowMessage("Press SPACE to launch\nMove: Left/Right or A/D");
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (!IsGameOver) return;

        if (kb.rKey.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        else if (kb.escapeKey.wasPressedThisFrame)
            SceneManager.LoadScene(menuSceneName);
    }

    public void RegisterBrick() => bricksRemaining++;

    public void OnBrickDestroyed(int points)
    {
        if (IsGameOver) return;
        Score += points;
        bricksRemaining--;
        RefreshUI();

        if (bricksRemaining <= 0)
        {
            ball.Stop();
            SoundManager.Clear();
            if (fireworks) fireworks.Play();
            EndGame("YOU WIN!");
        }
    }

    void EndGame(string title)
    {
        IsGameOver = true;
        int rank = HighScores.Submit(Score);
        string rankLine = rank > 0 ? $"\nNEW HIGH SCORE!  #{rank}" : "";
        ShowMessage($"{title}\nScore: {Score}{rankLine}\nR: Restart    ESC: Menu");
    }

    public void OnBallLost()
    {
        if (IsGameOver) return;
        Lives--;
        RefreshUI();

        if (Lives <= 0)
        {
            ball.Stop();
            ball.gameObject.SetActive(false);
            SoundManager.GameOver();
            EndGame("GAME OVER");
        }
        else
        {
            SoundManager.BallLost();
            ball.ResetToPaddle();
            ShowMessage("Press SPACE to launch");
        }
    }

    public void OnBallLaunched() => ShowMessage("");

    void RefreshUI()
    {
        if (scoreText) scoreText.text = $"SCORE  {Score}";
        if (livesText) livesText.text = $"LIVES  {Lives}";
    }

    void ShowMessage(string msg)
    {
        if (messageText) messageText.text = msg;
    }
}
