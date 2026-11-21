using System.Collections;
using System.Collections.Generic;
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
    public ItemManager itemManager;
    [Tooltip("게임 종료 화면의 RESTART / MENU 버튼 묶음 (터치용)")]
    public GameObject endButtons;

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

    public bool IsPlaying => !IsGameOver && !levelTransition;
    public float BallSpeedScale => itemManager ? itemManager.BallSpeedScale : 1f;
    public int BallCount => balls.Count;

    bool levelTransition;
    int bricksRemaining;
    readonly List<Ball> balls = new List<Ball>();

    void Awake()
    {
        Instance = this;
        Lives = startLives;
        Score = 0;
        if (ball) balls.Add(ball);
    }

    void Start()
    {
        if (endButtons) endButtons.SetActive(false);
        if (!ValidateReferences()) return;
        LoadLevel(1);
    }

    /// <summary>씬이 현재 코드보다 오래된 경우(빌더 재실행 필요) 조용히 망가지지 않도록 검사.</summary>
    bool ValidateReferences()
    {
        var missing = new List<string>();
        if (!ball) missing.Add(nameof(ball));
        if (!brickField) missing.Add(nameof(brickField));
        if (!itemManager) missing.Add(nameof(itemManager));
        if (!endButtons) missing.Add(nameof(endButtons));
        if (missing.Count == 0) return true;

        string msg = $"Scene is out of date (missing: {string.Join(", ", missing)}).\nRun Tools > Build Breakout Scene";
        Debug.LogError($"[Breakout] {msg.Replace('\n', ' ')}", this);
        ShowMessage(msg);
        if (ball) ball.Stop();
        IsGameOver = true; // 입력 차단 (R/ESC만 동작)
        return false;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (!IsGameOver) return;

        if (kb.rKey.wasPressedThisFrame) Restart();
        else if (kb.escapeKey.wasPressedThisFrame) GoToMenu();
    }

    // 종료 화면 버튼(터치/마우스)과 키보드 R/ESC가 함께 사용
    public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void GoToMenu() => SceneManager.LoadScene(menuSceneName);

    void LoadLevel(int level)
    {
        Level = level;
        levelTransition = false;
        bricksRemaining = brickField.Build(Levels.Get(level), level);
        itemManager.ResetAll();
        ResetToSingleBall(baseBallSpeed + speedPerLevel * (level - 1));
        RefreshUI();

        string help = level == 1 ? "\nMove: Drag  or  Left/Right" : "";
        ShowMessage($"LEVEL {level}\n{LaunchHint}{help}");
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
        StopAllBalls();
        itemManager.ResetAll();
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

    public void OnBallLost(Ball lost)
    {
        if (!IsPlaying || !balls.Contains(lost)) return;

        // 공이 여러 개면 떨어진 공만 제거 (목숨 유지)
        if (balls.Count > 1)
        {
            RemoveBall(lost);
            return;
        }

        Lives--;
        RefreshUI();
        itemManager.ResetAll();

        if (Lives <= 0)
        {
            StopAllBalls();
            foreach (var b in balls) b.gameObject.SetActive(false);
            SoundManager.GameOver();
            EndGame("GAME OVER");
        }
        else
        {
            SoundManager.BallLost();
            ResetToSingleBall(null);
            ShowMessage(LaunchHint);
        }
    }

    // ---------- 공 관리 (멀티볼) ----------

    /// <summary>공을 1개만 남기고 패들 위로 되돌린다. baseSpeed가 주어지면 레벨 기본 속도도 갱신.</summary>
    void ResetToSingleBall(float? baseSpeed)
    {
        for (int i = balls.Count - 1; i >= 1; i--)
            RemoveBall(balls[i]);
        ball = balls[0];
        if (baseSpeed.HasValue) ball.baseSpeed = baseSpeed.Value;
        ball.ResetToPaddle();
    }

    void StopAllBalls()
    {
        foreach (var b in balls) b.Stop();
    }

    void RemoveBall(Ball b)
    {
        balls.Remove(b);
        Destroy(b.gameObject);
        if (ball == b && balls.Count > 0) ball = balls[0];
    }

    /// <summary>+공 아이템: 날아가는 공(없으면 패들 위 공)에서 갈라져 나오는 공 추가.</summary>
    public void AddBalls(int count)
    {
        if (!IsPlaying || balls.Count == 0) return;

        Ball source = balls.Find(b => b.IsLaunched) ?? balls[0];
        Vector2 pos = source.transform.position;
        Vector2 dir = source.IsLaunched ? source.Direction : Vector2.up;
        float speed = source.IsLaunched ? source.CurrentSpeed : source.baseSpeed;

        for (int i = 0; i < count; i++)
        {
            float angle = (i % 2 == 0 ? 1f : -1f) * (25f + 10f * (i / 2));
            Vector2 d = Quaternion.Euler(0f, 0f, angle) * dir;
            if (d.y < 0.2f && !source.IsLaunched) d.y = 0.2f; // 패들 위에서 나올 때는 위쪽으로

            var clone = Instantiate(source, pos, Quaternion.identity);
            clone.name = "Ball (extra)";
            // 공끼리는 부딪히지 않게
            foreach (var other in balls)
                Physics2D.IgnoreCollision(clone.Collider, other.Collider);
            clone.LaunchFrom(pos, d, speed);
            balls.Add(clone);
        }
    }

    /// <summary>-공 아이템: 공 개수를 절반으로 (올림, 최소 1개).</summary>
    public void HalveBalls()
    {
        if (!IsPlaying) return;
        int target = Mathf.Max(1, (balls.Count + 1) / 2);
        while (balls.Count > target)
            RemoveBall(balls[Random.Range(0, balls.Count)]);
    }

    void EndGame(string title)
    {
        IsGameOver = true;
        itemManager.ResetAll();
        int rank = HighScores.Submit(Score);
        string rankLine = rank > 0 ? $"\nNEW HIGH SCORE!  #{rank}" : "";
        ShowMessage($"{title}\nScore: {Score}{rankLine}");
        if (endButtons) endButtons.SetActive(true);
    }

    const string LaunchHint = "Tap or press SPACE to launch";

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
