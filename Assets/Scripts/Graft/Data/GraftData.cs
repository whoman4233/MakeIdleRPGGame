using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 장착할 괴이 육체 데이터
[CreateAssetMenu(fileName = "NewGraftData", menuName = "Anomaly/Graft Data")]
public class GraftData : ScriptableObject
{
    public string graftName;
    [TextArea] public string description;
    public GraftSlotType slotType; // 이식할 신체 부위
    public Sprite icon;            // 인벤토리에 표시될 아이콘 이미지
    
    // 하나의 육체가 가지는 긍정적/부정적 스탯들
    public List<StatModifier> modifiers; 
}