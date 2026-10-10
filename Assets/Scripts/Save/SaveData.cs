using System;
using System.Collections.Generic;

/// <summary>
/// JSON으로 직렬화되는 세이브 데이터 구조체.
/// GraftData(SO)는 직접 직렬화할 수 없으므로 SO의 .name(에셋 파일명)으로 저장한다.
/// </summary>
[Serializable]
public class SaveData
{
    // --- 재화 ---
    public long dataAmount;
    public int coreAmount;

    // --- 인벤토리 (GraftData SO 이름 목록) ---
    public List<string> inventoryGraftIds = new List<string>();

    // --- 장착 슬롯 (GraftData SO 이름, 비어있으면 빈 문자열) ---
    public string equippedHead  = "";
    public string equippedCore  = "";
    public string equippedArmL  = "";
    public string equippedArmR  = "";
    public string equippedLegs  = "";

    // --- 업그레이드 레벨 (UpgradeManager.upgrades 배열과 동일 인덱스) ---
    public int[] upgradeLevels = new int[0];

    // --- 스테이지 진행도 ---
    public int currentStageIndex = 0;
    // --- 무한 심층 (엔드리스) ---
    public int endlessLoopCount = 0;   // 현재 심층 깊이 (엔드리스 반복 횟수)
    public int bestDepth = 0;          // 최고 도달 심층 기록
    public List<int> claimedDepthMilestones = new List<int>();  // 수령 완료한 깊이 마일스톤
    // --- 부위 강화 레벨 (graft.name 과 level 을 병렬 리스트로 저장) ---
    public List<string> graftEnhanceIds = new List<string>();
    public List<int> graftEnhanceLevels = new List<int>();



    // --- 추출 누적 횟수 (비용 곡선용) ---
    public int basicExtractionCount = 0;

    // --- 오프라인 보상용: 마지막 저장 시각 (UTC ISO 8601 문자열) ---
    public string lastSaveTimeUtc = "";
}
