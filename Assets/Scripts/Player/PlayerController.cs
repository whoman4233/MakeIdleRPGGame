using UnityEngine;

public enum PlayerStateType
{
    Idle,
    MoveForward,
    Chase,
    Attack,
    Dead
}

[RequireComponent(typeof(PlayerStats))]
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

    private IPlayerState _currentState;
    private IPlayerState
        _idleState, _moveState, _chaseState, _attackState, _deadState;

    private void Awake()
    {
        Stats = GetComponent<PlayerStats>();

        _idleState = new IdleState();
        _moveState = new MoveForwardState();
        _chaseState = new ChaseState();
        _attackState = new AttackState();
        _deadState = new DeadState();
    }

    private void OnEnable()
    {
        Stats.OnDied += OnDied;
    }

    private void OnDisable()
    {
        Stats.OnDied -= OnDied;
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

        foreach (var unit in registry.Units)
        {
            if (unit == null || !unit.IsAlive)
                continue;

            // ÆÀ Ã¼Å© ¡æ Stats.TeamId·Î º¯°æµÊ
            if (unit.TeamId == Stats.TeamId)
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
