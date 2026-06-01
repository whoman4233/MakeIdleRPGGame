using System;
using UnityEngine;
using GoogleMobileAds.Api;

/// <summary>
/// 괴이 격리 프로토콜 - AdMob 보상형 광고 매니저 (SDK v11)
///
/// [앱 ID 설정]
///   Unity 메뉴 > Google Mobile Ads > Settings
///   Android App ID 입력: ca-app-pub-xxxxxxxx~xxxxxxxxxx
///
/// [광고 단위 ID]
///   테스트: ca-app-pub-3940256099942544/5224354917
///   실제:   AdMob 콘솔 > 보상형 광고 단위 생성 후 rewardedAdUnitId 교체
/// </summary>
public class AdManager : MonoBehaviour, IRewardedAdService
{
    public static AdManager Instance { get; private set; }

    [Header("광고 단위 ID")]
    [Tooltip("실제 출시 전 AdMob 콘솔 ID로 교체")]
    public string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";

    [Header("더미 모드")]
    [Tooltip("에디터에서 true로 두면 광고 없이 즉시 보상")]
    public bool dummyMode = false;

    private RewardedAd _rewardedAd;
    private bool _isLoaded = false;

    private Action _onReward;
    private Action _onClosed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
#if UNITY_EDITOR
        dummyMode = true;
        GameLog.Log("[AdManager] 에디터 더미 모드");
        return;
#endif
        MobileAds.Initialize(_ =>
        {
            GameLog.Log("[AdManager] SDK 초기화 완료");
            LoadAd();
        });
    }

    // ── 광고 로드 ────────────────────────────────

    private void LoadAd()
    {
        _rewardedAd?.Destroy();
        _isLoaded = false;

        RewardedAd.Load(rewardedAdUnitId, new AdRequest(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                GameLog.Warn($"[AdManager] 로드 실패: {error?.GetMessage()}");
                return;
            }
            _rewardedAd = ad;
            _isLoaded = true;

            _rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                _isLoaded = false;
                _onClosed?.Invoke();
                LoadAd();
            };

            _rewardedAd.OnAdFullScreenContentFailed += err =>
            {
                _isLoaded = false;
                GameLog.Warn($"[AdManager] 표시 실패: {err.GetMessage()}");
                LoadAd();
            };

            GameLog.Log("[AdManager] 광고 로드 완료");
        });
    }

    // ── 공개 API ────────────────────────────────

    /// <summary>
    /// 보상형 광고 표시.
    /// 에디터(더미 모드)에서는 즉시 onReward 실행.
    /// </summary>
    public void ShowRewardedAd(Action onReward, Action onClosed = null)
    {
        _onReward = onReward;
        _onClosed = onClosed;

        if (dummyMode)
        {
            GameLog.Log("[AdManager] 더미: 즉시 보상");
            _onReward?.Invoke();
            _onClosed?.Invoke();
            return;
        }

        if (!_isLoaded || _rewardedAd == null)
        {
            GameLog.Warn("[AdManager] 광고 미준비 - 재로드");
            LoadAd();
            return;
        }

        _rewardedAd.Show(reward =>
        {
            GameLog.Log($"[AdManager] 보상: {reward.Type} x{reward.Amount}");
            _onReward?.Invoke();
        });
    }

    /// <summary>광고 준비 여부 (버튼 활성화용)</summary>
    public bool IsAdReady() => dummyMode || _isLoaded;
}