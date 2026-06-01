using System;
using UnityEngine;

public enum PlayerStateType
{
    Idle,
    MoveForward,
    Chase,
    Attack,
    Dead
}

[RequireComponent(typeof(PlayerStats), typeof(HealthSystem))]
public class PlayerController : MonoBehaviour
{
    [Header("Detection")]
    public float detectRange = 10f;
    public float attackRange = 2f;
    public LayerMask attackableLayers;

    [Header("Movement")]
    public Transform modelRoot;

    [Header("Debug")]
    public PlayerStateType currentStateType;

    public PlayerStats Stats { get; private set; }
    public IAttackable CurrentTarget { get; set; }
    public bool IsDead => currentStateType == PlayerStateType.Dead;

    private HealthSystem _healthSystem;
    private PlayerRigAnimator _rigAnimator;
    private IPlayerState _currentState;
    private IPlayerState _idleState, _moveState, _chaseState, _attackState, _deadState;

    // 타겟 탐색 쿨다운 (P1 개선 ⑥번)
    private float _targetSearchInterval = 0.15f;
    private float _nextSearchTime;
    private IAttackable _cachedTarget;

    private void Awake()
    {
        Stats = GetComponent<PlayerStats>();
        _rigAnimator = GetComponent<PlayerRigAnimator>();
        _healthSystem = GetComponent<HealthSystem>();
        _idleState   = new IdleState();
        _moveState   = new MoveForwardState();
        _chaseState  = new ChaseState();
        _attackState = new AttackState();
        _deadState   = new DeadState();
    }

    private void OnEnable()
    {
        if (_healthSystem != null) _healthSystem.OnDied += OnDied;
    }

    private void OnDisable()
    {
        if (_healthSystem != null) _healthSystem.OnDied -= OnDied;
    }

    private void Start()
    {
        ChangeState(PlayerStateType.MoveForward);
    }

    private void Update()
    {
        _currentState?.Tick();
    }

    private void OnDied()
    {
        ChangeState(PlayerStateType.Dead);
    }

    public void ChangeState(PlayerStateType newState)
    {
        // Dead 상태에서는 Revive 명시 없이 다른 상태로 전이 불가
        if (currentStateType == PlayerStateType.Dead && newState != PlayerStateType.MoveForward)
            return;

        if (currentStateType == newState && _currentState != null)
            return;

        _currentState?.Exit();
        currentStateType = newState;
        UpdateRigAnimation(newState);

        switch (newState)
        {
            case PlayerStateType.Idle:        _currentState = _idleState;   break;
            case PlayerStateType.MoveForward: _currentState = _moveState;   break;
            case PlayerStateType.Chase:       _currentState = _chaseState;  break;
            case PlayerStateType.Attack:      _currentState = _attackState; break;
            case PlayerStateType.Dead:        _currentState = _deadState;   break;
        }

        _currentState?.Enter(this);
    }

    #region Actions

    public void MoveForward()
    {
        if (modelRoot != null)
            modelRoot.rotation = Quaternion.LookRotation(Vector3.right);
    }

    public void MoveTowards(Vector3 targetPos)
    {
        if (modelRoot != null)
            modelRoot.rotation = Quaternion.LookRotation(Vector3.right);
    }

    // ⑥번 개선: 0.15초 캐시로 매 프레임 전체 순회 방지
    public IAttackable FindTarget()
    {
        if (Time.time < _nextSearchTime) return _cachedTarget;
        _nextSearchTime = Time.time + _targetSearchInterval;

        var registry = AttackableRegistry.Instance;
        if (registry == null) { _cachedTarget = null; return null; }

        float maxSqr  = detectRange * detectRange;
        float bestSqr = maxSqr;
        IAttackable best = null;
        int myTeamId = _healthSystem != null ? _healthSystem.TeamId : 0;

        foreach (var unit in registry.Units)
        {
            if (unit == null || !unit.IsAlive) continue;
            if (unit.TeamId == myTeamId) continue;
            Transform tr = unit.Transform;
            if (((1 << tr.gameObject.layer) & attackableLayers.value) == 0) continue;
            float sqr = (tr.position - transform.position).sqrMagnitude;
            if (sqr <= bestSqr) { bestSqr = sqr; best = unit; }
        }
        _cachedTarget = best;
        return best;
    }

    public float DistanceToTarget()
    {
        if (CurrentTarget == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position, CurrentTarget.Transform.position);
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private void UpdateRigAnimation(PlayerStateType state)
    {
        if (_rigAnimator == null) return;
        bool moving = (state == PlayerStateType.MoveForward || state == PlayerStateType.Chase);
        _rigAnimator.SetMoving(moving);
        if (state == PlayerStateType.Attack)
            _rigAnimator.TriggerAttack();
    }

}