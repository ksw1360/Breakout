using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    public Paddle paddle;
    public float speed = 8f;
    [Tooltip("패들 가장자리에 맞았을 때 최대 반사 각도(수직 기준)")]
    public float maxBounceAngle = 60f;
    [Tooltip("공이 너무 수평으로 날지 않도록 하는 최소 수직 성분 비율")]
    public float minVerticalRatio = 0.25f;

    Rigidbody2D rb;
    bool launched;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        ResetToPaddle();
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
        rb.linearVelocity = v * speed;
    }

    void Launch()
    {
        launched = true;
        float angle = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
        rb.linearVelocity = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * speed;
        GameManager.Instance?.OnBallLaunched();
    }

    public void ResetToPaddle()
    {
        launched = false;
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
            rb.linearVelocity = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * speed;
        }
    }
}
