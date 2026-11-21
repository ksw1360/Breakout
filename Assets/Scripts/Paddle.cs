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
    float input;

    public float HalfWidth => transform.localScale.x * 0.5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        pos.x = Mathf.Clamp(pos.x + input * speed * Time.fixedDeltaTime, minX, maxX);
        rb.MovePosition(pos);
    }
}
