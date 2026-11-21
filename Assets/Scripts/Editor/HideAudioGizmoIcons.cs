using UnityEditor;
using UnityEngine;

/// <summary>
/// SoundManager(AudioSource)와 카메라(AudioListener)의 스피커 기즈모 아이콘이
/// Game 뷰 가운데에 보이지 않도록 끈다. 에디터 전용 표시라 빌드에는 영향 없음.
/// </summary>
static class HideAudioGizmoIcons
{
    [InitializeOnLoadMethod]
    static void Apply()
    {
        EditorApplication.delayCall += () =>
        {
            GizmoUtility.SetIconEnabled(typeof(AudioSource), false);
            GizmoUtility.SetIconEnabled(typeof(AudioListener), false);
        };
    }
}
