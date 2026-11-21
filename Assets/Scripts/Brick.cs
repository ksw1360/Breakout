using UnityEngine;

public class Brick : MonoBehaviour
{
    // 단단한 블럭의 남은 횟수별 색 (index = 남은 횟수)
    static readonly Color[] HardColors =
    {
        Color.clear,
        new Color(0.86f, 0.89f, 0.95f), // 1번 남음: 밝은 은색
        new Color(0.62f, 0.68f, 0.80f), // 2번 남음
        new Color(0.40f, 0.45f, 0.57f), // 3번 남음: 어두운 강철
    };

    public int points = 10;
    public int hitPoints = 1;

    bool hard;
    bool destroyed;
    SpriteRenderer sr;

    public void Init(int hp, int pts, Color normalColor)
    {
        hitPoints = hp;
        points = pts;
        hard = hp > 1;
        sr = GetComponent<SpriteRenderer>();
        sr.color = hard ? HardColors[hp] : normalColor;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (destroyed) return;
        if (collision.collider.GetComponent<Ball>() == null) return;

        hitPoints--;
        if (hitPoints > 0)
        {
            SoundManager.HardHit();
            if (sr) sr.color = HardColors[Mathf.Clamp(hitPoints, 1, HardColors.Length - 1)];
            return;
        }

        destroyed = true;
        SoundManager.BrickHit();
        GameManager.Instance?.OnBrickDestroyed(points);
        Destroy(gameObject);
    }
}
