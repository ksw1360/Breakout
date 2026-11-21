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

    [Header("Rules")]
    public int startLives = 3;

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

        if (IsGameOver && kb.rKey.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
            IsGameOver = true;
            ball.Stop();
            ShowMessage($"YOU WIN!\nScore: {Score}\nPress R to restart");
        }
    }

    public void OnBallLost()
    {
        if (IsGameOver) return;
        Lives--;
        RefreshUI();

        if (Lives <= 0)
        {
            IsGameOver = true;
            ball.Stop();
            ball.gameObject.SetActive(false);
            ShowMessage($"GAME OVER\nScore: {Score}\nPress R to restart");
        }
        else
        {
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
