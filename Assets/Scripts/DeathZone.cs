using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DeathZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        var ball = other.GetComponent<Ball>();
        if (ball != null && GameManager.Instance)
            GameManager.Instance.OnBallLost(ball);
    }
}
