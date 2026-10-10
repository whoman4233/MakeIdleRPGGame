using System;
using UnityEngine;

/// <summary>
/// 오프라인(방치) 보상 매니저.
/// 마지막 저장 시각과 현재 시각의 차이를 계산해, 초당 획득량 × 경과시간만큼 재화(Data)를 지급한다.
/// 누적 상한은 maxOfflineHours로 제한한다. (계산식: 초당 × 시간 — 단순 방식)
/// </summary>
public class OfflineRewardManager : MonoBehaviour
{
    public static OfflineRewardManager Instance { get; private set; }

    [Header("보상 설정")]
    [Tooltip("기본 초당 Data 획득량 (스테이지 배율이 곱해짐)")]
    [SerializeField] private float baseDataPerSecond = 2f;
    [Tooltip("오프라인 보상 최대 누적 시간 (이 이상 경과해도 더 적립되지 않음)")]
    [SerializeField] private float maxOfflineHours = 4f;
    [Tooltip("이 시간(초) 미만 경과 시에는 보상 팝업을 띄우지 않음")]
    [SerializeField] private float minOfflineSeconds = 60f;

    /// <summary>마지막 계산된 오프라인 보상 결과 (UI 표시용)</summary>
    public long   LastRewardData    { get; private set; }
    public double LastOfflineSeconds { get; private set; }

    /// <summary>보상이 지급되었을 때 발생 (UI 팝업이 구독). (보상량, 경과초)</summary>
    public event Action<long, double> OnOfflineRewardGranted;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // SaveManager의 로드가 Start에서 일어나므로, 한 프레임 뒤에 계산
        StartCoroutine(EvaluateNextFrame());
    }

    private System.Collections.IEnumerator EvaluateNextFrame()
    {
        yield return null; // SaveManager.LoadGame() 완료 대기
        Evaluate();
    }

    private void Evaluate()
    {
        var sm = SaveManager.Instance;
        if (sm == null || sm.LastLoadedData == null) return; // 세이브 없음(첫 실행)

        string iso = sm.LastLoadedData.lastSaveTimeUtc;
        if (string.IsNullOrEmpty(iso)) return;

        if (!DateTime.TryParse(iso, null,
            System.Globalization.DateTimeStyles.RoundtripKind, out var lastUtc))
            return;

        double elapsed = (DateTime.UtcNow - lastUtc).TotalSeconds;
        if (elapsed < minOfflineSeconds) return; // 너무 짧으면 무시

        // 상한 적용
        double capped = Math.Min(elapsed, maxOfflineHours * 3600.0);

        // 초당 획득량 × 스테이지 배율
        float mult = StageManager.Instance != null ? StageManager.Instance.GetStatMultiplier() : 1f;
        long reward = (long)(baseDataPerSecond * mult * capped);
        if (reward <= 0) return;

        CurrencyManager.Instance?.AddData(reward);

        LastRewardData     = reward;
        LastOfflineSeconds = capped;
        OnOfflineRewardGranted?.Invoke(reward, capped);

        GameLog.Log($"[Offline] 경과 {capped/60.0:F1}분 → Data +{reward} 지급 (배율 {mult:F2})");
    }
}
