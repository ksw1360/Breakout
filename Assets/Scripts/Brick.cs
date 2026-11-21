using UnityEngine;

public class Brick : MonoBehaviour
{
    public int points = 10;
    bool destroyed;

    void Start()
    {
        GameManager.Instance?.RegisterBrick();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (destroyed) return;
        if (collision.collider.GetComponent<Ball>() == null) return;

        destroyed = true;
        GameManager.Instance?.OnBrickDestroyed(points);
        Destroy(gameObject);
    }
}
