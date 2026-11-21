using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

/// <summary>
/// Build Settings의 씬 순서를 Menu(0) → Breakout(1) → 나머지 로 맞춘다.
/// 씬 빌더 메뉴 실행 시, 그리고 스크립트 컴파일 후 자동으로 실행된다.
/// </summary>
public static class BuildSettingsOrder
{
    public const string MenuScene = "Assets/Scenes/Menu.unity";
    public const string GameScene = "Assets/Scenes/Breakout.unity";
    static readonly string[] Ordered = { MenuScene, GameScene };

    [InitializeOnLoadMethod]
    static void AutoApply() => EditorApplication.delayCall += Apply;

    [MenuItem("Tools/Fix Build Scene Order")]
    public static void Apply()
    {
        var current = EditorBuildSettings.scenes;
        var result = new List<EditorBuildSettingsScene>();

        foreach (var path in Ordered)
        {
            // AssetDatabase는 SaveScene 직후 아직 GUID를 모를 수 있으므로 디스크 파일로 판단
            if (!File.Exists(path))
                continue; // 아직 생성되지 않은 씬
            result.Add(new EditorBuildSettingsScene(path, true));
        }
        result.AddRange(current.Where(s => !Ordered.Contains(s.path)));

        bool changed = result.Count != current.Length ||
                       result.Where((s, i) => s.path != current[i].path || s.enabled != current[i].enabled).Any();
        if (changed)
        {
            EditorBuildSettings.scenes = result.ToArray();
            AssetDatabase.SaveAssets(); // ProjectSettings를 디스크에 반영
        }
    }
}
