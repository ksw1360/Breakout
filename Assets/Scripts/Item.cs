using UnityEngine;

public enum ItemType
{
    MultiBall,   // 초록 "+" : 공 2개 추가
    ReduceBalls, // 빨강 "-" : 공 절반 (최소 1개)
    WidePaddle,  // 파랑 "W" : 패들 확장
    SlowBall,    // 노랑 "S" : 공 느리게
}

/// <summary>떨어지는 아이템. 물리 대신 직접 이동 + 패들 바운즈 검사로 처리.</summary>
public class Item : MonoBehaviour
{
    public ItemType Type { get; private set; }

    ItemManager owner;
    SpriteRenderer sr;
    float fallSpeed;

    public void Init(ItemManager owner, ItemType type, Sprite sprite, Material material, float fallSpeed)
    {
        this.owner = owner;
        this.fallSpeed = fallSpeed;
        Type = type;
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        if (material) sr.sharedMaterial = material;
        sr.sortingOrder = 5;
    }

    void Update()
    {
        transform.position += Vector3.down * (fallSpeed * Time.deltaTime);

        if (owner.paddle && sr.bounds.Intersects(owner.paddle.Bounds))
            owner.OnItemCaught(this);
        else if (transform.position.y < owner.missY)
            owner.OnItemMissed(this);
    }
}
