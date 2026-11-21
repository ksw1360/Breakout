using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 터치(또는 마우스) 입력을 게임용으로 해석한다.
///  - 드래그: 이번 프레임의 가로 이동량 (월드 단위) → 패들 이동
///  - 탭: 짧게 눌렀다 뗐고 거의 움직이지 않음 → 공 발사
/// 여러 스크립트가 같은 프레임에 호출해도 프레임당 한 번만 계산된다.
/// </summary>
public static class PointerInput
{
    const float TapMaxDuration = 0.3f;
    const float TapMaxMoveRatio = 0.03f; // 화면 높이 대비 허용 이동량

    static int lastFrame = -1;
    static bool pressing;
    static bool dragged;
    static Vector2 startPos, lastPos;
    static float startTime;
    static float dragDeltaPixels;
    static bool tapped;

    /// <summary>이번 프레임 가로 드래그 이동량 (월드 단위, 직교 카메라 기준).</summary>
    public static float DragDeltaWorldX
    {
        get
        {
            Refresh();
            var cam = Camera.main;
            if (cam == null || !cam.orthographic || Screen.height <= 0) return 0f;
            return dragDeltaPixels * (cam.orthographicSize * 2f / Screen.height);
        }
    }

    /// <summary>이번 프레임에 탭이 끝났는지.</summary>
    public static bool Tapped
    {
        get { Refresh(); return tapped; }
    }

    // 씬 전환 시 상태 초기화: 버튼을 누른 손가락을 뗀 것이 새 씬에서 '탭'으로 잡혀 공이 발사되는 것을 방지
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        ResetState();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        ResetState();
        // 이미 눌려 있는 포인터는 새로 시작된 누름으로 보지 않음 (드래그로만 취급)
        var pointer = Pointer.current;
        if (pointer != null && pointer.press.isPressed)
        {
            pressing = true;
            dragged = true;
            lastPos = startPos = pointer.position.ReadValue();
        }
    }

    static void ResetState()
    {
        lastFrame = -1;
        pressing = dragged = tapped = false;
        dragDeltaPixels = 0f;
    }

    static void Refresh()
    {
        if (Time.frameCount == lastFrame) return;
        lastFrame = Time.frameCount;
        dragDeltaPixels = 0f;
        tapped = false;

        var pointer = Pointer.current; // 폰에선 Touchscreen, PC에선 Mouse
        if (pointer == null)
        {
            pressing = false;
            return;
        }

        bool down = pointer.press.isPressed;
        Vector2 pos = pointer.position.ReadValue();

        if (down && !pressing)
        {
            pressing = true;
            dragged = false;
            startPos = lastPos = pos;
            startTime = Time.unscaledTime;
        }
        else if (down)
        {
            dragDeltaPixels = pos.x - lastPos.x;
            lastPos = pos;
            if ((pos - startPos).magnitude > Screen.height * TapMaxMoveRatio)
                dragged = true;
        }
        else if (pressing)
        {
            pressing = false;
            tapped = !dragged && Time.unscaledTime - startTime <= TapMaxDuration;
        }
    }
}
