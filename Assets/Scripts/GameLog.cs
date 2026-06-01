using System.Diagnostics;
using UnityEngine;

/// <summary>
/// 게임 전역 로그 유틸리티.
/// [Conditional] 어트리뷰트로 UNITY_EDITOR 또는 개발 빌드에서만 컴파일된다.
/// 릴리즈 빌드에서는 호출부 자체가 컴파일 단계에서 제거되어,
/// 인자 문자열 연결 비용까지 발생하지 않는다.
/// </summary>
public static class GameLog
{
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message, Object context = null)
        => UnityEngine.Debug.Log(message, context);

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Warn(object message, Object context = null)
        => UnityEngine.Debug.LogWarning(message, context);

    // 에러는 릴리즈에서도 남긴다 (크래시 추적용) — Conditional 없음
    public static void Error(object message, Object context = null)
        => UnityEngine.Debug.LogError(message, context);
}
