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
    public float forwardSpeed = 3f;

    [Header("Debug")]
    public PlayerStateType currentStateType;

    public PlayerStats Stats { get; private set; }
    public IAttackable CurrentTarget { get; set; }

    private HealthSystem _healthSystem; // HealthSystem 참조 추가
    private IPlayerState _currentState;
    private IPlayerState
        _idleState, _moveState, _chaseState, _attackState, _deadState;

    private void Awake()
    {
        Stats = GetComponent<PlayerStats>();
        _healthSystem = GetComponent<HealthSystem>(); // 초기화

        _idleState = new IdleState();
        _moveState = new MoveForwardState();
        _chaseState = new ChaseState();
        _attackState = new AttackState();
        _deadState = new DeadState();
    }

    private void OnEnable()
    {
        if (_healthSystem != null)
        {
            // HealthSystem의 사망 이벤트 구독
            _healthSystem.OnDied += OnDied;
        }
    }

    private void OnDisable()
    {
        if (_healthSystem != null)
        {
            // HealthSystem의 사망 이벤트 구독 해제
            _healthSystem.OnDied -= OnDied;
        }
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
        if (currentStateType == newState && _currentState != null)
            return;

        _currentState?.Exit();

        currentStateType = newState;

        switch (newState)
        {
            case PlayerStateType.Idle: _currentState = _idleState; break;
            case PlayerStateType.MoveForward: _currentState = _moveState; break;
            case PlayerStateType.Chase: _currentState = _chaseState; break;
            case PlayerStateType.Attack: _currentState = _attackState; break;
            case PlayerStateType.Dead: _currentState = _deadState; break;
        }

        _currentState?.Enter(this);
    }

    #region Actions

    public void MoveForward()
    {
        Vector3 dir = Vector3.forward;
        transform.Translate(dir * forwardSpeed * Time.deltaTime, Space.World);

        if (modelRoot != null && dir != Vector3.zero)
            modelRoot.rotation = Quaternion.LookRotation(dir);
    }

    public void MoveTowards(Vector3 targetPos)
    {
        Vector3 toTarget = targetPos - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude <= 0.0001f)
            return;

        Vector3 dir = toTarget.normalized;
        transform.Translate(dir * forwardSpeed * Time.deltaTime, Space.World);

        if (modelRoot != null)
            modelRoot.rotation = Quaternion.LookRotation(dir);
    }

    public IAttackable FindTarget()
    {
        var registry = AttackableRegistry.Instance;
        if (registry == null) return null;

        float maxSqr = detectRange * detectRange;
        float bestSqr = maxSqr;
        IAttackable best = null;

        // 임시로 Player의 팀 ID를 0이라고 가정하여 하드코딩 혹은 HealthSystem을 참조하도록 처리할 수 있습니다.
        // 기존 Stats.TeamId 부분은 게임 로직에 따라 알맞은 위치에서 가져오게 수정이 필요할 수 있습니다.
        int myTeamId = _healthSystem != null ? _healthSystem.TeamId : 0;

        foreach (var unit in registry.Units)
        {
            if (unit == null || !unit.IsAlive)
                continue;

            // 적 체크 시 팀 ID 비교
            if (unit.TeamId == myTeamId)
                continue;

            Transform tr = unit.Transform;

            if (((1 << tr.gameObject.layer) & attackableLayers.value) == 0)
                continue;

            float sqr = (tr.position - transform.position).sqrMagnitude;
            if (sqr <= bestSqr)
            {
                bestSqr = sqr;
                best = unit;
            }
        }

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
}