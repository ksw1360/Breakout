using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MenuSceneBuilder
{
    const string ScenePath = BuildSettingsOrder.MenuScene;

    static readonly Color ButtonColor = new Color(0.20f, 0.24f, 0.34f);

    [MenuItem("Tools/Build Menu Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // 카메라
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.07f, 0.08f, 0.12f);
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();

        // 이벤트 시스템 (새 Input System 전용 프로젝트이므로 InputSystemUIInputModule 사용)
        CreateEventSystem();

        // 캔버스
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var menu = canvasGo.AddComponent<MainMenu>();

        var title = BreakoutSceneBuilder.CreateText("Title", canvasGo.transform, font, 140, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 300f));
        title.text = "BREAKOUT";
        title.fontStyle = FontStyle.Bold;
        title.color = new Color(0.98f, 0.85f, 0.30f);

        // 메인 패널
        var mainPanel = CreatePanel("MainPanel", canvasGo.transform);
        var startBtn = CreateButton("StartButton", mainPanel.transform, font, "START", new Vector2(0f, 40f));
        var scoresBtn = CreateButton("ScoresButton", mainPanel.transform, font, "HIGH SCORES", new Vector2(0f, -80f));
        var quitBtn = CreateButton("QuitButton", mainPanel.transform, font, "QUIT", new Vector2(0f, -200f));
        UnityEventTools.AddPersistentListener(startBtn.onClick, new UnityAction(menu.StartGame));
        UnityEventTools.AddPersistentListener(scoresBtn.onClick, new UnityAction(menu.ShowScores));
        UnityEventTools.AddPersistentListener(quitBtn.onClick, new UnityAction(menu.Quit));

        // 점수 패널
        var scoresPanel = CreatePanel("ScoresPanel", canvasGo.transform);
        var header = BreakoutSceneBuilder.CreateText("Header", scoresPanel.transform, font, 64, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 130f));
        header.text = $"HIGH SCORES  TOP {HighScores.MaxEntries}";
        var scoresText = BreakoutSceneBuilder.CreateText("ScoresText", scoresPanel.transform, font, 52, TextAnchor.UpperCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 70f));
        scoresText.rectTransform.pivot = new Vector2(0.5f, 1f);
        scoresText.lineSpacing = 1.1f;
        var backBtn = CreateButton("BackButton", scoresPanel.transform, font, "BACK", new Vector2(0f, -380f));
        UnityEventTools.AddPersistentListener(backBtn.onClick, new UnityAction(menu.ShowMain));
        scoresPanel.SetActive(false);

        menu.mainPanel = mainPanel;
        menu.scoresPanel = scoresPanel;
        menu.scoresText = scoresText;
        menu.startButton = startBtn;
        menu.backButton = backBtn;

        EditorSceneManager.SaveScene(scene, ScenePath);
        BuildSettingsOrder.Apply();

        Debug.Log($"[Breakout] 메뉴 씬 생성 완료: {ScenePath}");
    }

    static Color Shade(float k) => new Color(ButtonColor.r * k, ButtonColor.g * k, ButtonColor.b * k, 1f);

    static GameObject CreatePanel(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    /// <summary>새 Input System용 EventSystem (마우스 + 터치 UI 입력).</summary>
    internal static void CreateEventSystem()
    {
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }

    internal static Button CreateButton(string name, Transform parent, Font font, string label, Vector2 position)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(480f, 96f);

        var image = go.GetComponent<Image>();
        image.color = Color.white;

        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = ButtonColor;
        colors.highlightedColor = Shade(1.6f);
        colors.selectedColor = Shade(1.6f);
        colors.pressedColor = Shade(0.7f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        var text = BreakoutSceneBuilder.CreateText("Label", go.transform, font, 48, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), Vector2.zero);
        text.text = label;
        var trt = text.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        return button;
    }

}
