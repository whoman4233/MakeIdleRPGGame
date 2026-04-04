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
    // forwardSpeed 변수는 더 이상 좌표 이동에 쓰이지 않으므로 제거하거나,
    // 나중에 애니메이션 재생 속도 조절용으로 쓸 수 있게 남겨둘 수 있습니다. (여기서는 제거)

    [Header("Debug")]
    public PlayerStateType currentStateType;

    public PlayerStats Stats { get; private set; }
    public IAttackable CurrentTarget { get; set; }

    private HealthSystem _healthSystem; 
    private IPlayerState _currentState;
    private IPlayerState _idleState, _moveState, _chaseState, _attackState, _deadState;

    private void Awake()
    {
        Stats = GetComponent<PlayerStats>();
        _healthSystem = GetComponent<HealthSystem>(); 

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
            _healthSystem.OnDied += OnDied;
        }
    }

    private void OnDisable()
    {
        if (_healthSystem != null)
        {
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
        // 런닝머신 기믹: 실제 좌표 이동은 멈추고, 우측을 바라보도록 방향만 고정합니다.
        Vector3 dir = Vector3.right; 
        
        if (modelRoot != null)
            modelRoot.rotation = Quaternion.LookRotation(dir);
    }

    public void MoveTowards(Vector3 targetPos)
    {
        // 런닝머신 기믹: 적을 추적하는 상태(Chase)가 되더라도 이동하지 않습니다.
        // 적이 스스로 다가오기 때문에 방향만 우측으로 유지합니다.
        Vector3 dir = Vector3.right; 

        if (modelRoot != null)
        {
            modelRoot.rotation = Quaternion.LookRotation(dir);
        }
    }

    public IAttackable FindTarget()
    {
        var registry = AttackableRegistry.Instance;
        if (registry == null) return null;

        float maxSqr = detectRange * detectRange;
        float bestSqr = maxSqr;
        IAttackable best = null;

        int myTeamId = _healthSystem != null ? _healthSystem.TeamId : 0;

        foreach (var unit in registry.Units)
        {
            if (unit == null || !unit.IsAlive)
                continue;

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