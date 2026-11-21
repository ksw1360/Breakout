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

    AudioSource source;

    void Awake()
    {
        Instance = this;
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
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
    public static void BrickHit()  => PlayStatic(m => m.brickHit);
    public static void BallLost()  => PlayStatic(m => m.ballLost);
    public static void GameOver()  => PlayStatic(m => m.gameOver);
    public static void Clear()     => PlayStatic(m => m.clear);
}
