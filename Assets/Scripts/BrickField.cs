using UnityEngine;

/// <summary>레벨 배치 문자열을 읽어 블럭들을 자식으로 생성한다.</summary>
public class BrickField : MonoBehaviour
{
    public Sprite sprite;
    public Material material;

    [Header("Layout")]
    public float brickWidth = 1.3f;
    public float brickHeight = 0.45f;
    public float gap = 0.1f;
    public float topY = 4.5f;

    [Header("Scoring")]
    [Tooltip("맞아야 하는 횟수별 기본 점수 (index = 횟수). 레벨 번호를 곱한다.")]
    public int[] basePoints = { 0, 10, 30, 50 };

    public Color[] rowColors =
    {
        new Color(0.95f, 0.30f, 0.30f), // 빨강
        new Color(0.98f, 0.60f, 0.25f), // 주황
        new Color(0.98f, 0.85f, 0.30f), // 노랑
        new Color(0.40f, 0.85f, 0.40f), // 초록
        new Color(0.35f, 0.65f, 0.98f), // 파랑
        new Color(0.70f, 0.45f, 0.95f), // 보라
    };

    /// <summary>배치를 생성하고 생성된 블럭 수를 반환.</summary>
    public int Build(string[] layout, int level)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        int count = 0;
        for (int row = 0; row < layout.Length; row++)
        {
            string line = layout[row];
            float totalWidth = line.Length * brickWidth + (line.Length - 1) * gap;
            float startX = -totalWidth * 0.5f + brickWidth * 0.5f;
            float y = topY - row * (brickHeight + gap);

            for (int col = 0; col < line.Length; col++)
            {
                int hp = line[col] - '0';
                if (hp < 1 || hp > 3) continue;

                var go = new GameObject($"Brick_{row}_{col}");
                go.transform.SetParent(transform, false);
                go.transform.position = new Vector3(startX + col * (brickWidth + gap), y, 0f);
                go.transform.localScale = new Vector3(brickWidth, brickHeight, 1f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                if (material) sr.sharedMaterial = material;
                go.AddComponent<BoxCollider2D>();

                var brick = go.AddComponent<Brick>();
                int points = basePoints[Mathf.Min(hp, basePoints.Length - 1)] * level;
                brick.Init(hp, points, rowColors[row % rowColors.Length]);
                count++;
            }
        }
        return count;
    }
}
