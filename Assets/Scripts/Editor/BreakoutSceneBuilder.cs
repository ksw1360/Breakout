using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BreakoutSceneBuilder
{
    const string SpriteFolder = "Assets/Sprites";
    const string PhysicsFolder = "Assets/Physics";
    const string SquarePath = SpriteFolder + "/Square.png";
    const string CirclePath = SpriteFolder + "/Circle.png";
    const string BounceMatPath = PhysicsFolder + "/BallBounce.physicsMaterial2D";
    const string ScenePath = "Assets/Scenes/Breakout.unity";
    const string UnlitMatPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

    // 플레이 영역 (월드 좌표)
    const float CameraSize = 6f;
    const float ArenaHalfWidth = 7.5f;   // 좌우 벽 안쪽 x
    const float ArenaTop = 5.5f;         // 천장 안쪽 y
    const float WallThickness = 0.5f;

    // 블럭 배치
    const int Columns = 10;
    const int Rows = 6;
    const float BrickWidth = 1.3f;
    const float BrickHeight = 0.45f;
    const float BrickGap = 0.1f;
    const float BrickTopY = 4.5f;

    static readonly Color[] RowColors =
    {
        new Color(0.95f, 0.30f, 0.30f), // 빨강
        new Color(0.98f, 0.60f, 0.25f), // 주황
        new Color(0.98f, 0.85f, 0.30f), // 노랑
        new Color(0.40f, 0.85f, 0.40f), // 초록
        new Color(0.35f, 0.65f, 0.98f), // 파랑
        new Color(0.70f, 0.45f, 0.95f), // 보라
    };

    [MenuItem("Tools/Build Breakout Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Sprite square = GetOrCreateSprite(SquarePath, 32, false);
        Sprite circle = GetOrCreateSprite(CirclePath, 64, true);
        PhysicsMaterial2D bounce = GetOrCreateBounceMaterial();
        Material spriteMat = AssetDatabase.LoadAssetAtPath<Material>(UnlitMatPath);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 카메라
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = CameraSize;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.07f, 0.08f, 0.12f);
        camGo.transform.position = new Vector3(0f, 0f, -10f);

        // 벽 (좌, 우, 천장)
        var walls = new GameObject("Walls");
        Color wallColor = new Color(0.55f, 0.58f, 0.65f);
        float wallHeight = ArenaTop + CameraSize + 1f;
        float wallCenterY = (ArenaTop - CameraSize - 1f) * 0.5f;
        CreateBlock("Wall Left", walls.transform, square, spriteMat, wallColor,
            new Vector2(-ArenaHalfWidth - WallThickness * 0.5f, wallCenterY),
            new Vector2(WallThickness, wallHeight));
        CreateBlock("Wall Right", walls.transform, square, spriteMat, wallColor,
            new Vector2(ArenaHalfWidth + WallThickness * 0.5f, wallCenterY),
            new Vector2(WallThickness, wallHeight));
        CreateBlock("Wall Top", walls.transform, square, spriteMat, wallColor,
            new Vector2(0f, ArenaTop + WallThickness * 0.5f),
            new Vector2(ArenaHalfWidth * 2f + WallThickness * 2f, WallThickness));

        // 바닥 아래 데스존 (트리거)
        var death = new GameObject("DeathZone");
        death.transform.position = new Vector3(0f, -CameraSize - 1f, 0f);
        var deathCol = death.AddComponent<BoxCollider2D>();
        deathCol.isTrigger = true;
        deathCol.size = new Vector2(ArenaHalfWidth * 2f + 4f, 1f);
        death.AddComponent<DeathZone>();

        // 패들
        Vector2 paddleSize = new Vector2(2f, 0.3f);
        var paddleGo = CreateBlock("Paddle", null, square, spriteMat, new Color(0.9f, 0.92f, 0.95f),
            new Vector2(0f, -4.8f), paddleSize);
        var paddleRb = paddleGo.AddComponent<Rigidbody2D>();
        paddleRb.bodyType = RigidbodyType2D.Kinematic;
        paddleRb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var paddle = paddleGo.AddComponent<Paddle>();
        paddle.minX = -ArenaHalfWidth + paddleSize.x * 0.5f;
        paddle.maxX = ArenaHalfWidth - paddleSize.x * 0.5f;

        // 공
        var ballGo = new GameObject("Ball");
        ballGo.transform.position = new Vector3(0f, -4.35f, 0f);
        ballGo.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
        var ballSr = ballGo.AddComponent<SpriteRenderer>();
        ballSr.sprite = circle;
        if (spriteMat) ballSr.sharedMaterial = spriteMat;
        ballSr.sortingOrder = 1;
        var ballCol = ballGo.AddComponent<CircleCollider2D>();
        ballCol.sharedMaterial = bounce;
        var ballRb = ballGo.AddComponent<Rigidbody2D>();
        ballRb.gravityScale = 0f;
        ballRb.linearDamping = 0f;
        ballRb.angularDamping = 0f;
        ballRb.freezeRotation = true;
        ballRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        ballRb.interpolation = RigidbodyInterpolation2D.Interpolate;
        ballRb.sharedMaterial = bounce;
        var ball = ballGo.AddComponent<Ball>();
        ball.paddle = paddle;

        // 블럭
        var bricks = new GameObject("Bricks");
        float totalWidth = Columns * BrickWidth + (Columns - 1) * BrickGap;
        float startX = -totalWidth * 0.5f + BrickWidth * 0.5f;
        for (int row = 0; row < Rows; row++)
        {
            float y = BrickTopY - row * (BrickHeight + BrickGap);
            for (int col = 0; col < Columns; col++)
            {
                float x = startX + col * (BrickWidth + BrickGap);
                var brickGo = CreateBlock($"Brick_{row}_{col}", bricks.transform, square, spriteMat,
                    RowColors[row % RowColors.Length], new Vector2(x, y), new Vector2(BrickWidth, BrickHeight));
                var brick = brickGo.AddComponent<Brick>();
                brick.points = (Rows - row) * 10; // 위쪽 줄일수록 점수 높음
            }
        }

        // UI
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var scoreText = CreateText("ScoreText", canvasGo.transform, font, 40, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(30f, -20f));
        var livesText = CreateText("LivesText", canvasGo.transform, font, 40, TextAnchor.UpperRight,
            new Vector2(1f, 1f), new Vector2(-30f, -20f));
        var messageText = CreateText("MessageText", canvasGo.transform, font, 56, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0f, -120f));
        messageText.rectTransform.sizeDelta = new Vector2(1200f, 400f);

        // 게임 매니저
        var gmGo = new GameObject("GameManager");
        var gm = gmGo.AddComponent<GameManager>();
        gm.ball = ball;
        gm.scoreText = scoreText;
        gm.livesText = livesText;
        gm.messageText = messageText;

        // 씬 저장 + 빌드 설정에 등록
        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);

        Debug.Log($"[Breakout] 씬 생성 완료: {ScenePath} — Play 버튼을 누르세요.");
    }

    static GameObject CreateBlock(string name, Transform parent, Sprite sprite, Material mat, Color color,
        Vector2 position, Vector2 size)
    {
        var go = new GameObject(name);
        if (parent) go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        if (mat) sr.sharedMaterial = mat;
        go.AddComponent<BoxCollider2D>(); // 스프라이트 크기(1x1)에 맞춰 자동 설정, 스케일 적용됨
        return go;
    }

    static Text CreateText(string name, Transform parent, Font font, int size, TextAnchor anchor,
        Vector2 pivotAnchor, Vector2 offset)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = pivotAnchor;
        rt.anchoredPosition = offset;
        rt.sizeDelta = new Vector2(600f, 80f);
        var text = go.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.alignment = anchor;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    static Sprite GetOrCreateSprite(string path, int size, bool circle)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing) return existing;

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        float r = size * 0.5f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            byte a = 255;
            if (circle)
            {
                float dx = x + 0.5f - r, dy = y + 0.5f - r;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                a = (byte)(Mathf.Clamp01(r - d) * 255f); // 가장자리 안티앨리어싱
            }
            pixels[y * size + x] = new Color32(255, 255, 255, a);
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = size; // 1 유닛 = 텍스처 전체
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = circle ? FilterMode.Bilinear : FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static PhysicsMaterial2D GetOrCreateBounceMaterial()
    {
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(BounceMatPath);
        if (mat) return mat;

        Directory.CreateDirectory(PhysicsFolder);
        mat = new PhysicsMaterial2D("BallBounce") { bounciness = 1f, friction = 0f };
        AssetDatabase.CreateAsset(mat, BounceMatPath);
        AssetDatabase.SaveAssets();
        return mat;
    }

    static void AddSceneToBuildSettings(string path)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        scenes.RemoveAll(s => s.path == path);
        scenes.Insert(0, new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
