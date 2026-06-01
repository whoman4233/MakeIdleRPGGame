using UnityEngine;

/// <summary>
/// 코어 슬롯 장착 시 발동 가능한 스킬 데이터.
/// GraftData(Core 슬롯)에 연결해 사용.
/// </summary>
[CreateAssetMenu(menuName = "Anomaly/Core Skill Data", fileName = "NewCoreSkill")]
public class CoreSkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName     = "내부 파열";
    [TextArea]
    public string description   = "코어 내부의 이물질을 폭발시켜 주변 적에게 피해를 준다.";

    [Header("Stats")]
    [Tooltip("공격력 대비 데미지 배율")]
    public float  damageMultiplier = 3f;
    [Tooltip("적용 반경 (월드 단위)")]
    public float  radius           = 5f;
    [Tooltip("쿨다운 (초)")]
    public float  cooldown         = 8f;

    [Header("Visual")]
    [Tooltip("스킬 아이콘 (UI 버튼에 표시)")]
    public Sprite icon;
    [Tooltip("발동 시 플레이어 위치에 재생되는 파티클 프리팹 (없어도 됨)")]
    public GameObject vfxPrefab;
}
