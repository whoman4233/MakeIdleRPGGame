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

    // --- 추출 누적 횟수 (비용 곡선용) ---
    public int basicExtractionCount = 0;
}
