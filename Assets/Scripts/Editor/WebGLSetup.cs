using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// itch.io 업로드용 WebGL Player Settings.
///  - 캔버스 960x600
///  - Gzip 압축 + Decompression Fallback (itch.io는 Content-Encoding 헤더를 안 붙여주므로 필요)
/// 메뉴로 즉시 적용할 수 있고, WebGL 빌드 직전에도 자동으로 강제 적용된다.
/// </summary>
public class WebGLSetup : IPreprocessBuildWithReport
{
    public const int Width = 960;
    public const int Height = 600;

    public int callbackOrder => 0;

    [MenuItem("Tools/Configure WebGL for itch.io")]
    public static void Apply()
    {
        PlayerSettings.defaultWebScreenWidth = Width;
        PlayerSettings.defaultWebScreenHeight = Height;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.runInBackground = false; // 탭이 가려지면 일시정지
        AssetDatabase.SaveAssets();

        Debug.Log($"[Breakout] WebGL 설정 적용: {Width}x{Height}, Gzip, Decompression Fallback ON");
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.WebGL)
            Apply();
    }
}
