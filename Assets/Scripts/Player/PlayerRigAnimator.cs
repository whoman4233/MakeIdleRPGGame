using UnityEngine;

/// <summary>
/// 측면 플레이어 본 애니메이션 (코드 기반, Animator 불필요).
/// 골반 축 다리 스윙 + 어깨 축 팔 스윙 + 몸통 상하 바운스로 걷기 표현.
/// 공격 시 앞팔(ArmFront)을 휘두름.
///
/// [본 계층]
///   Rig > Hips > (Torso > (Head, ArmBack, ArmFront), LegBack, LegFront)
/// </summary>
public class PlayerRigAnimator : MonoBehaviour
{
    [Header("Bones (비우면 자동 탐색)")]
    public Transform hips;
    public Transform torso;
    public Transform head;
    public Transform armFront;
    public Transform armBack;
    public Transform legFront;
    public Transform legBack;

    [Header("Walk Settings")]
    [Tooltip("걷기 주기 속도")]
    public float walkSpeed = 7f;
    [Tooltip("다리 스윙 각도")]
    public float legSwing = 22f;
    [Tooltip("팔 스윙 각도")]
    public float armSwing = 16f;
    [Tooltip("몸통 상하 바운스 높이")]
    public float bounce = 0.06f;

    [Header("Idle Settings")]
    public float idleSpeed = 2.5f;
    public float idleSway  = 3f;

    [Header("Attack Settings")]
    public float attackDuration = 0.35f;
    public float attackSwing    = 70f;

    private bool   _isMoving;
    private float  _t;
    private float  _attackTimer;
    private Vector3 _hipsBasePos;
    private float  _headBaseY;

    private void Awake() => AutoFind();

    private void AutoFind()
    {
        if (hips == null)
        {
            var rig = transform.Find("Model/Rig");
            if (rig == null) return;
            hips = rig.Find("Hips");
        }
        if (hips == null) return;
        if (torso    == null) torso    = hips.Find("Torso");
        if (legBack  == null) legBack  = hips.Find("LegBack");
        if (legFront == null) legFront = hips.Find("LegFront");
        if (torso != null)
        {
            if (head     == null) head     = torso.Find("Head");
            if (armBack  == null) armBack  = torso.Find("ArmBack");
            if (armFront == null) armFront = torso.Find("ArmFront");
        }
        if (hips != null) _hipsBasePos = hips.localPosition;
    }

    private void Start()
    {
        if (hips != null) _hipsBasePos = hips.localPosition;
    }

    /// <summary>이동 상태 설정 (PlayerController에서 호출)</summary>
    public void SetMoving(bool moving) => _isMoving = moving;

    /// <summary>공격 모션 트리거</summary>
    public void TriggerAttack() => _attackTimer = attackDuration;

    private void LateUpdate()
    {
        if (hips == null) return;

        // 공격 모션 우선
        if (_attackTimer > 0f)
        {
            _attackTimer -= Time.deltaTime;
            float prog = 1f - (_attackTimer / attackDuration); // 0~1
            // 앞으로 휘둘렀다 돌아오기 (sin 0~pi)
            float swing = Mathf.Sin(prog * Mathf.PI) * attackSwing;
            if (armFront != null) armFront.localRotation = Quaternion.Euler(0,0,-swing);
            // 공격 중 몸통 살짝 앞으로
            if (torso != null) torso.localRotation = Quaternion.Euler(0,0,-swing*0.15f);
            return;
        }
        else
        {
            if (torso != null) torso.localRotation = Quaternion.identity;
        }

        if (_isMoving)
        {
            _t += Time.deltaTime * walkSpeed;
            float s = Mathf.Sin(_t);
            float s2 = Mathf.Abs(Mathf.Cos(_t)); // 바운스용(2배 주기)

            if (legFront != null) legFront.localRotation = Quaternion.Euler(0,0,-s * legSwing);
            if (legBack  != null) legBack.localRotation  = Quaternion.Euler(0,0, s * legSwing);
            if (armFront != null) armFront.localRotation = Quaternion.Euler(0,0, s * armSwing);
            if (armBack  != null) armBack.localRotation  = Quaternion.Euler(0,0,-s * armSwing);

            // 몸통 상하 바운스
            Vector3 hp = _hipsBasePos; hp.y += s2 * bounce;
            hips.localPosition = hp;
        }
        else
        {
            // 대기: 가벼운 흔들림
            _t += Time.deltaTime * idleSpeed;
            float s = Mathf.Sin(_t);
            if (legFront != null) legFront.localRotation = Quaternion.identity;
            if (legBack  != null) legBack.localRotation  = Quaternion.identity;
            if (armFront != null) armFront.localRotation = Quaternion.Euler(0,0, s * idleSway);
            if (armBack  != null) armBack.localRotation  = Quaternion.Euler(0,0,-s * idleSway);
            if (head != null)     head.localRotation     = Quaternion.Euler(0,0, s * idleSway * 0.4f);
            hips.localPosition = _hipsBasePos;
        }
    }
}
