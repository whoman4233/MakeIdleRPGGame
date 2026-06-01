using System;
using UnityEngine;

/// <summary>
/// 코어 Graft 장착 시 활성화되는 스킬 핸들러.
/// PlayerGraft.OnGraftChanged 구독 → 코어 교체 시 자동 갱신.
/// Use() 를 외부(UI 버튼)에서 호출.
/// </summary>
[RequireComponent(typeof(PlayerGraft))]
[RequireComponent(typeof(PlayerStats))]
public class CoreSkillHandler : MonoBehaviour
{
    // 쿨다운 진행률 (0=사용 가능, 1=막 사용함)
    public event Action OnSkillChanged;   // 스킬 교체됨 (UI 갱신용)
    public event Action OnCooldownTick;   // 매 프레임 쿨다운 변경 (UI 갱신용)

    public CoreSkillData CurrentSkill  { get; private set; }
    public float         CooldownRatio { get; private set; }  // 0~1
    public bool          IsReady       => CooldownRatio <= 0f && CurrentSkill != null;
    public float         RemainSeconds => CurrentSkill != null
                                          ? CooldownRatio * CurrentSkill.cooldown : 0f;

    private PlayerGraft  _graft;
    private PlayerStats  _stats;
    private float        _cooldownTimer; // 남은 쿨다운 초

    private void Awake()
    {
        _graft = GetComponent<PlayerGraft>();
        _stats = GetComponent<PlayerStats>();
    }

    private void OnEnable()
    {
        if (_graft != null) _graft.OnGraftChanged += RefreshSkill;
    }

    private void OnDisable()
    {
        if (_graft != null) _graft.OnGraftChanged -= RefreshSkill;
    }

    private void Start() => RefreshSkill();

    private void Update()
    {
        if (_cooldownTimer <= 0f) return;
        _cooldownTimer  = Mathf.Max(0f, _cooldownTimer - Time.deltaTime);
        CooldownRatio   = CurrentSkill != null && CurrentSkill.cooldown > 0f
                          ? _cooldownTimer / CurrentSkill.cooldown : 0f;
        OnCooldownTick?.Invoke();
    }

    // ── 스킬 교체 ──────────────────────────────────────
    private void RefreshSkill()
    {
        var coreGraft = _graft?.GetEquipped(GraftSlotType.Core);
        var newSkill  = coreGraft?.coreSkill;

        if (newSkill == CurrentSkill) return;
        CurrentSkill   = newSkill;
        _cooldownTimer = 0f;
        CooldownRatio  = 0f;
        OnSkillChanged?.Invoke();
        OnCooldownTick?.Invoke();
    }

    // ── 스킬 사용 (UI 버튼에서 호출) ──────────────────
    public void Use()
    {
        if (!IsReady) return;

        // 데미지 계산
        float atk    = _stats.GetStatValue(StatType.AttackPower);
        float dmg    = atk * CurrentSkill.damageMultiplier;
        float radius = CurrentSkill.radius;

        // 범위 내 적 타격
        int hitCount = 0;
        var registry = AttackableRegistry.Instance;
        if (registry != null)
        {
            var enemies = registry.GetEnemiesInRange(transform.position, radius);
            foreach (var e in enemies)
            {
                if (e == null || !e.IsAlive) continue;
                e.TakeDamage(dmg);
                DamagePopupManager.Instance?.Show(e.Transform.position, dmg, false);
                hitCount++;
            }
        }

        // VFX
        if (CurrentSkill.vfxPrefab != null)
            Instantiate(CurrentSkill.vfxPrefab, transform.position, Quaternion.identity);

        // 사운드 (PlayerHit 재활용, 추후 전용 사운드로 교체 가능)
        if (hitCount > 0) SoundManager.Instance?.PlayEnemyHit();

        // 쿨다운 시작
        _cooldownTimer = CurrentSkill.cooldown;
        CooldownRatio  = 1f;
        OnCooldownTick?.Invoke();

        GameLog.Log($"[CoreSkill] {CurrentSkill.skillName} 발동 — 범위:{radius} 피해:{dmg:F0} 적:{hitCount}마리");
    }

    // ── 기즈모 ─────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (CurrentSkill == null) return;
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, CurrentSkill.radius);
    }
}
