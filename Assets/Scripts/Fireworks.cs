using System.Collections;
using UnityEngine;

/// <summary>코드로 파티클 시스템을 만들어 여러 색의 폭죽을 터뜨린다.</summary>
public class Fireworks : MonoBehaviour
{
    [Tooltip("원형 텍스처를 쓰는 스프라이트(Unlit) 머티리얼")]
    public Material particleMaterial;

    [Header("Show")]
    public float showDuration = 2.2f;
    public float burstInterval = 0.22f;
    public Vector2 areaMin = new Vector2(-6f, -0.5f);
    public Vector2 areaMax = new Vector2(6f, 4f);

    [Header("Burst")]
    public int particlesPerBurst = 90;
    public Vector2 speedRange = new Vector2(2.5f, 6f);
    public Vector2 lifetimeRange = new Vector2(0.8f, 1.4f);
    public Vector2 sizeRange = new Vector2(0.10f, 0.22f);

    static readonly Color[] Palette =
    {
        new Color(1.00f, 0.35f, 0.35f),
        new Color(1.00f, 0.75f, 0.25f),
        new Color(1.00f, 1.00f, 0.45f),
        new Color(0.45f, 1.00f, 0.50f),
        new Color(0.40f, 0.80f, 1.00f),
        new Color(0.75f, 0.50f, 1.00f),
        new Color(1.00f, 0.50f, 0.90f),
    };

    public bool IsPlaying { get; private set; }

    /// <summary>폭죽 쇼 시작. 마지막 불꽃이 사라질 때까지 기다리는 총 시간을 반환.</summary>
    public float Play()
    {
        StopAllCoroutines();
        StartCoroutine(Show());
        return showDuration + lifetimeRange.y;
    }

    IEnumerator Show()
    {
        IsPlaying = true;
        float elapsed = 0f;
        while (elapsed < showDuration)
        {
            var pos = new Vector2(Random.Range(areaMin.x, areaMax.x), Random.Range(areaMin.y, areaMax.y));
            SpawnBurst(pos);
            float wait = burstInterval * Random.Range(0.6f, 1.4f);
            yield return new WaitForSeconds(wait);
            elapsed += wait;
        }
        yield return new WaitForSeconds(lifetimeRange.y);
        IsPlaying = false;
    }

    void SpawnBurst(Vector2 position)
    {
        var go = new GameObject("FireworkBurst");
        go.transform.SetParent(transform, false);
        go.transform.position = position;

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // 설정 변경 전 정지 필요

        Color a = Palette[Random.Range(0, Palette.Length)];
        Color b = Palette[Random.Range(0, Palette.Length)];

        var main = ps.main;
        main.duration = 0.1f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetimeRange.x, lifetimeRange.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speedRange.x, speedRange.y);
        main.startSize = new ParticleSystem.MinMaxCurve(sizeRange.x, sizeRange.y);
        main.startColor = new ParticleSystem.MinMaxGradient(a, b);
        main.gravityModifier = 0.6f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = particlesPerBurst;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)particlesPerBurst) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.05f;

        // 공기 저항 느낌으로 감속
        var limit = ps.limitVelocityOverLifetime;
        limit.enabled = true;
        limit.drag = 1.5f;

        // 서서히 사라지고 작아지기
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.3f));

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        if (particleMaterial) renderer.sharedMaterial = particleMaterial;
        renderer.sortingOrder = 10;

        ps.Play();
    }
}
