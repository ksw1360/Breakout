using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    public Paddle paddle;
    [Tooltip("레벨 기본 속도 (GameManager가 레벨마다 설정)")]
    [FormerlySerializedAs("speed")] public float baseSpeed = 8f;
    [Tooltip("패들/블럭에 맞을 때마다 늘어나는 속도")]
    public float speedStepPerHit = 0.12f;
    [Tooltip("최대 속도 = 기본 속도 × 이 값")]
    public float maxSpeedMultiplier = 1.5f;
    [Tooltip("패들 가장자리에 맞았을 때 최대 반사 각도(수직 기준)")]
    public float maxBounceAngle = 60f;
    [Tooltip("공이 너무 수평으로 날지 않도록 하는 최소 수직 성분 비율")]
    public float minVerticalRatio = 0.25f;

    Rigidbody2D rb;
    bool launched;

    public float CurrentSpeed { get; private set; }
    public float MaxSpeed => baseSpeed * maxSpeedMultiplier;
    public bool IsLaunched => launched;
    public Vector2 Direction => rb.linearVelocity.sqrMagnitude > 0.0001f ? rb.linearVelocity.normalized : Vector2.up;
    public Collider2D Collider { get; private set; }

    // 느리게 아이템 등 전역 배율을 적용한 실제 속도
    float EffectiveSpeed => CurrentSpeed * (GameManager.Instance ? GameManager.Instance.BallSpeedScale : 1f);

    void SpeedUp() => CurrentSpeed = Mathf.Min(CurrentSpeed + speedStepPerHit, MaxSpeed);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Collider = GetComponent<Collider2D>();
        CurrentSpeed = baseSpeed;
    }

    void Update()
    {
        if (launched || paddle == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        var kb = Keyboard.current;
        if (kb != null && kb.spaceKey.wasPressedThisFrame)
            Launch();
    }

    void FixedUpdate()
    {
        if (!launched)
        {
            if (paddle != null)
                rb.position = (Vector2)paddle.transform.position + Vector2.up * 0.45f;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 일정 속도 유지 + 너무 수평으로 흐르는 것 방지
        Vector2 v = rb.linearVelocity;
        if (v.sqrMagnitude < 0.0001f) v = Vector2.up;
        v.Normalize();
        if (Mathf.Abs(v.y) < minVerticalRatio)
        {
            v.y = minVerticalRatio * (v.y >= 0f ? 1f : -1f);
            v.x = Mathf.Sign(v.x) * Mathf.Sqrt(1f - v.y * v.y);
        }
        rb.linearVelocity = v * EffectiveSpeed;
    }

    void Launch()
    {
        launched = true;
        float angle = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
        rb.linearVelocity = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * EffectiveSpeed;
        GameManager.Instance?.OnBallLaunched();
    }

    /// <summary>멀티볼용: 지정 위치에서 지정 방향/속도로 바로 발사된 상태로 만든다.</summary>
    public void LaunchFrom(Vector2 position, Vector2 direction, float speed)
    {
        gameObject.SetActive(true);
        enabled = true;
        launched = true;
        CurrentSpeed = Mathf.Min(speed, MaxSpeed);
        rb.position = position;
        transform.position = position;
        rb.linearVelocity = direction.normalized * EffectiveSpeed;
    }

    public void ResetToPaddle()
    {
        gameObject.SetActive(true);
        enabled = true;
        launched = false;
        CurrentSpeed = baseSpeed;
        rb.linearVelocity = Vector2.zero;
        if (paddle != null)
            rb.position = (Vector2)paddle.transform.position + Vector2.up * 0.45f;
    }

    public void Stop()
    {
        launched = false;
        rb.linearVelocity = Vector2.zero;
        enabled = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!launched) return;

        // 패들에 맞은 위치에 따라 반사 각도를 조절
        var hitPaddle = collision.collider.GetComponent<Paddle>();
        if (hitPaddle != null)
        {
            float offset = (rb.position.x - hitPaddle.transform.position.x) / hitPaddle.HalfWidth;
            offset = Mathf.Clamp(offset, -1f, 1f);
            float angle = offset * maxBounceAngle * Mathf.Deg2Rad;
            SpeedUp();
            rb.linearVelocity = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * EffectiveSpeed;
            SoundManager.PaddleHit();
        }
        else if (collision.collider.GetComponent<Brick>() != null)
        {
            // 블럭 소리는 Brick이 직접 재생 (깨짐 / 단단한 블럭 타격 구분)
            SpeedUp(); // 속도는 다음 FixedUpdate에서 반영
        }
        else
        {
            SoundManager.WallHit();
        }
    }
}
