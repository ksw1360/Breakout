using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public Ball ball;
    public BrickField brickField;
    public Text scoreText;
    public Text livesText;
    public Text levelText;
    public Text messageText;
    public Fireworks fireworks;

    [Header("Rules")]
    public int startLives = 3;
    public string menuSceneName = "Menu";
    public float baseBallSpeed = 8f;
    [Tooltip("레벨이 하나 오를 때마다 늘어나는 공 속도")]
    public float speedPerLevel = 0.8f;
    [Tooltip("스테이지 클리어 후 다음 레벨까지 최소 대기 시간(폭죽이 더 길면 폭죽 기준)")]
    public float minLevelClearDelay = 2.5f;

    public int Score { get; private set; }
    public int Lives { get; private set; }
    public int Level { get; private set; }
    public bool IsGameOver { get; private set; }

    bool levelTransition;
    int bricksRemaining;

    void Awake()
    {
        Instance = this;
        Lives = startLives;
        Score = 0;
    }

    void Start()
    {
        LoadLevel(1);
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

    void LoadLevel(int level)
    {
        Level = level;
        levelTransition = false;
        bricksRemaining = brickField.Build(Levels.Get(level), level);
        ball.speed = baseBallSpeed + speedPerLevel * (level - 1);
        ball.ResetToPaddle();
        RefreshUI();

        string help = level == 1 ? "\nMove: Left/Right or A/D" : "";
        ShowMessage($"LEVEL {level}\nPress SPACE to launch{help}");
    }

    public void OnBrickDestroyed(int points)
    {
        if (IsGameOver || levelTransition) return;
        Score += points;
        bricksRemaining--;
        RefreshUI();

        if (bricksRemaining <= 0)
            StageClear();
    }

    void StageClear()
    {
        ball.Stop();
        SoundManager.Clear();
        float fireworksTime = fireworks ? fireworks.Play() : 0f;

        if (Level >= Levels.Count)
        {
            EndGame("ALL CLEAR!");
            return;
        }

        levelTransition = true;
        ShowMessage($"LEVEL {Level} CLEAR!");
        StartCoroutine(NextLevelAfter(Mathf.Max(minLevelClearDelay, fireworksTime)));
    }

    IEnumerator NextLevelAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadLevel(Level + 1);
    }

    public void OnBallLost()
    {
        if (IsGameOver || levelTransition) return;
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

    void EndGame(string title)
    {
        IsGameOver = true;
        int rank = HighScores.Submit(Score);
        string rankLine = rank > 0 ? $"\nNEW HIGH SCORE!  #{rank}" : "";
        ShowMessage($"{title}\nScore: {Score}{rankLine}\nR: Restart    ESC: Menu");
    }

    public void OnBallLaunched() => ShowMessage("");

    void RefreshUI()
    {
        if (scoreText) scoreText.text = $"SCORE  {Score}";
        if (livesText) livesText.text = $"LIVES  {Lives}";
        if (levelText) levelText.text = $"LEVEL  {Level} / {Levels.Count}";
    }

    void ShowMessage(string msg)
    {
        if (messageText) messageText.text = msg;
    }
}
