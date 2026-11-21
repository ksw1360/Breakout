using System.Collections.Generic;
using UnityEngine;

/// <summary>아이템 드랍/획득/효과 지속시간 관리.</summary>
public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    public Paddle paddle;
    [Tooltip("스프라이트(Unlit) 머티리얼")]
    public Material material;

    [Header("Drop")]
    [Range(0f, 1f)] public float dropChance = 0.15f;
    public float fallSpeed = 2.5f;
    [Tooltip("이 y보다 내려가면 놓친 것으로 처리")]
    public float missY = -6.5f;
    public float itemWidth = 0.9f;

    [Header("Effects")]
    public int extraBalls = 2;
    public float wideMultiplier = 1.5f;
    public float wideDuration = 10f;
    [Range(0.1f, 1f)] public float slowMultiplier = 0.6f;
    public float slowDuration = 5f;

    [Header("Debug")]
    [Tooltip("I 키로 패들 위에 랜덤 아이템 떨어뜨리기 (테스트용)")]
    public bool debugSpawnKey = true;
    public float debugSpawnHeight = 3f;

    public float BallSpeedScale => slowTimer > 0f ? slowMultiplier : 1f;

    readonly List<Item> activeItems = new List<Item>();
    Dictionary<ItemType, Sprite> sprites;
    float wideTimer;
    float slowTimer;

    void Awake()
    {
        Instance = this;
        sprites = new Dictionary<ItemType, Sprite>
        {
            { ItemType.MultiBall,   ItemSprites.Create(new Color(0.30f, 0.82f, 0.36f), '+', itemWidth) },
            { ItemType.ReduceBalls, ItemSprites.Create(new Color(0.90f, 0.28f, 0.28f), '-', itemWidth) },
            { ItemType.WidePaddle,  ItemSprites.Create(new Color(0.30f, 0.55f, 0.95f), 'W', itemWidth) },
            { ItemType.SlowBall,    ItemSprites.Create(new Color(0.96f, 0.82f, 0.22f), 'S', itemWidth) },
        };
    }

    void OnDestroy()
    {
        // 런타임에 만든 텍스처/스프라이트 정리 (씬 재시작 시 누수 방지)
        if (sprites == null) return;
        foreach (var sp in sprites.Values)
        {
            if (!sp) continue;
            Destroy(sp.texture);
            Destroy(sp);
        }
    }

    void Update()
    {
        // 테스트용: I 키 → 패들 위로 랜덤 아이템
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (debugSpawnKey && kb != null && kb.iKey.wasPressedThisFrame && IsPlaying && paddle)
            Spawn(RandomType(), (Vector2)paddle.transform.position + Vector2.up * debugSpawnHeight);

        if (wideTimer > 0f)
        {
            wideTimer -= Time.deltaTime;
            if (wideTimer <= 0f && paddle) paddle.SetWidthMultiplier(1f);
        }
        if (slowTimer > 0f)
            slowTimer -= Time.deltaTime;
    }

    static bool IsPlaying => GameManager.Instance && GameManager.Instance.IsPlaying;

    /// <summary>블럭이 깨졌을 때 확률적으로 아이템 드랍.</summary>
    public void TryDrop(Vector2 position)
    {
        if (!IsPlaying || Random.value >= dropChance) return;
        Spawn(RandomType(), position);
    }

    public static ItemType RandomType() => (ItemType)Random.Range(0, System.Enum.GetValues(typeof(ItemType)).Length);

    public Item Spawn(ItemType type, Vector2 position)
    {
        var go = new GameObject($"Item_{type}");
        go.transform.SetParent(transform, false);
        go.transform.position = position;
        var item = go.AddComponent<Item>();
        item.Init(this, type, sprites[type], material, fallSpeed);
        activeItems.Add(item);
        return item;
    }

    public void OnItemCaught(Item item)
    {
        Remove(item);
        if (IsPlaying) Apply(item.Type);
    }

    public void OnItemMissed(Item item) => Remove(item);

    void Remove(Item item)
    {
        activeItems.Remove(item);
        Destroy(item.gameObject);
    }

    void Apply(ItemType type)
    {
        var gm = GameManager.Instance;
        switch (type)
        {
            case ItemType.MultiBall:
                gm.AddBalls(extraBalls);
                SoundManager.PowerUp();
                break;
            case ItemType.ReduceBalls:
                gm.HalveBalls();
                SoundManager.PowerDown();
                break;
            case ItemType.WidePaddle:
                wideTimer = wideDuration; // 다시 먹으면 시간만 갱신
                if (paddle) paddle.SetWidthMultiplier(wideMultiplier);
                SoundManager.PowerUp();
                break;
            case ItemType.SlowBall:
                slowTimer = slowDuration;
                SoundManager.PowerUp();
                break;
        }
    }

    /// <summary>떨어지는 아이템 제거 + 모든 효과 해제 (레벨 전환, 목숨 잃음, 게임 종료).</summary>
    public void ResetAll()
    {
        foreach (var item in activeItems)
            if (item) Destroy(item.gameObject);
        activeItems.Clear();
        wideTimer = 0f;
        slowTimer = 0f;
        if (paddle) paddle.SetWidthMultiplier(1f);
    }
}
