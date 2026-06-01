using System.Collections.Generic;
using UnityEngine;

// 에디터에서 우클릭으로 쉽게 데이터를 생성할 수 있도록 메뉴 속성 추가
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Anomaly/Enemy Data")]
public class EnemyStatsData : ScriptableObject
{
    [Header("Basic Info")]
    public string id = "enemy_default";
    
    [Header("Visuals (2.5D)")]
    public string enemyName = "미확인 표본";      // UI나 로그에 띄울 적 이름
    public Sprite enemySprite;                    // 적 2D 이미지 (스포너가 껍데기에 씌워줄 이미지)
    public Vector3 visualScale = Vector3.one;     // 크기 조절용 (보스는 크게, 잡몹은 작게)
    public Color visualColor = Color.white;       // 색상 베리에이션 (팔레트 스왑용)

    [Header("Stats")]
    public float MaxHealth = 50f;                 // 기존 maxHP
    public float AttackPower = 5f;                // 기존 attackPower
    public float MoveSpeed = 2f;                  // 기존 0이었으나 런닝머신을 위해 기본값 2로 세팅
    public float AttackInterval = 1.5f;           // 기존 attackInterval
    public float AttackRange = 1.5f;              // 기존 attackRange

    [Header("Reward")]
    // EXP 시스템이 폐기되었으므로 expReward는 완전히 삭제했습니다.
    public int DropGold = 5;                      // 기존 goldReward (관측 데이터 드랍량)

    [Header("Graft Drop")]
    [Tooltip("적 사망 시 Graft 드랍 확률 (0~1)")]
    public float graftDropChance = 0.15f;   // 일반 몹 기본 15%
    [Tooltip("드랍할 Graft 목록. 비어있으면 드랍 없음")]
    public List<GraftData> dropGrafts = new List<GraftData>();

    [Header("Flags")]
    public bool isBoss = false; 
}