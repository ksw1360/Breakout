using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public AudioClip paddleHit;
    public AudioClip wallHit;
    public AudioClip brickHit;
    public AudioClip ballLost;
    public AudioClip gameOver;
    public AudioClip clear;

    [Range(0f, 1f)] public float volume = 0.8f;

    [Header("Brick (glass)")]
    [Range(0f, 1f)] public float brickVolume = 0.45f;
    public float brickPitchMin = 0.88f;
    public float brickPitchMax = 1.15f;
    [Tooltip("연속으로 깨질 때 소리가 겹칠 수 있도록 쓰는 AudioSource 개수")]
    public int brickVoices = 4;

    AudioSource source;
    AudioSource[] brickSources;
    int nextBrickSource;

    void Awake()
    {
        Instance = this;
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;

        // pitch는 AudioSource 단위라서, 블럭 소리 전용 소스를 여러 개 두고 돌려 쓴다
        brickSources = new AudioSource[Mathf.Max(1, brickVoices)];
        for (int i = 0; i < brickSources.Length; i++)
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            brickSources[i] = s;
        }
    }

    void PlayBrick()
    {
        if (brickHit == null) return;
        var s = brickSources[nextBrickSource];
        nextBrickSource = (nextBrickSource + 1) % brickSources.Length;
        s.Stop();
        s.pitch = Random.Range(brickPitchMin, brickPitchMax);
        s.clip = brickHit;
        s.volume = volume * brickVolume;
        s.Play();
    }

    void Play(AudioClip clip)
    {
        if (clip != null) source.PlayOneShot(clip, volume);
    }

    // 씬 전환 직후 파괴된 인스턴스를 참조하지 않도록 Unity null 체크 사용
    static void PlayStatic(System.Func<SoundManager, AudioClip> pick)
    {
        if (Instance) Instance.Play(pick(Instance));
    }

    public static void PaddleHit() => PlayStatic(m => m.paddleHit);
    public static void WallHit()   => PlayStatic(m => m.wallHit);
    public static void BrickHit()
    {
        if (Instance) Instance.PlayBrick();
    }
    public static void BallLost()  => PlayStatic(m => m.ballLost);
    public static void GameOver()  => PlayStatic(m => m.gameOver);
    public static void Clear()     => PlayStatic(m => m.clear);
}
