using System;

/// <summary>
/// 보상형 광고 서비스 추상화.
/// AdMob / Unity Ads / 더미 등 광고 백엔드를 동일 인터페이스로 교체 가능하게 한다.
/// 게임 로직(부활, 보상 2배 등)은 구체 SDK가 아닌 이 인터페이스에만 의존한다.
/// </summary>
public interface IRewardedAdService
{
    /// <summary>광고 준비 여부 (버튼 활성화 판단용)</summary>
    bool IsAdReady();

    /// <summary>보상형 광고 표시. 보상 획득 시 onReward, 닫힘 시 onClosed 호출.</summary>
    void ShowRewardedAd(Action onReward, Action onClosed = null);
}
