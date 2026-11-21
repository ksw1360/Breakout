using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Paddle : MonoBehaviour
{
    public float speed = 12f;
    [Tooltip("패들 중심이 갈 수 있는 x 범위")]
    public float minX = -6.5f;
    public float maxX = 6.5f;

    Rigidbody2D rb;
    Collider2D col;
    float input;
    float baseScaleX;
    float arenaMin, arenaMax; // 벽 안쪽 경계 (패들 폭이 바뀌어도 유지)

    public float HalfWidth => transform.localScale.x * 0.5f;
    public Bounds Bounds => col.bounds;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        baseScaleX = transform.localScale.x;
        // minX/maxX는 기본 폭 기준의 중심 범위 → 벽 경계로 변환
        arenaMin = minX - baseScaleX * 0.5f;
        arenaMax = maxX + baseScaleX * 0.5f;
    }

    /// <summary>기본 폭 대비 배율로 패들 폭 변경 (1 = 원래 크기).</summary>
    public void SetWidthMultiplier(float multiplier)
    {
        var s = transform.localScale;
        s.x = baseScaleX * multiplier;
        transform.localScale = s;
    }

    void Update()
    {
        input = 0f;
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) input -= 1f;
        if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) input += 1f;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        Vector2 pos = rb.position;
        pos.x = Mathf.Clamp(pos.x + input * speed * Time.fixedDeltaTime, arenaMin + HalfWidth, arenaMax - HalfWidth);
        rb.MovePosition(pos);
    }
}
