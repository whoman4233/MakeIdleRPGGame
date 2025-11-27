using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyStats))]
public class EnemyController : MonoBehaviour
{
    public EnemyStats Stats { get; private set; }

    [Header("View")]
    public Transform modelRoot;

    [Header("AI")]
    [Tooltip("거리 체크 주기 (초)")]
    public float thinkInterval = 0.2f;

    private IAttackable _player;
    private Coroutine _attackRoutine;
    private float _thinkTimer;

    private void Awake()
    {
        Stats = GetComponent<EnemyStats>();
    }

    private void Start()
    {
        // PlayerStats가 IAttackable 구현했으니까 그대로 타겟으로 사용
        if (PlayerRef.Instance != null)
            _player = PlayerRef.Instance.Stats;
    }

    private void Update()
    {
        if (!Stats.IsAlive)
        {
            StopAttackRoutine();
            return;
        }

        if (_player == null || !_player.IsAlive)
        {
            StopAttackRoutine();
            return;
        }

        _thinkTimer -= Time.deltaTime;
        if (_thinkTimer > 0f)
            return;

        _thinkTimer = thinkInterval;

        float sqr = (_player.Transform.position - transform.position).sqrMagnitude;
        float range = Stats.AttackRange;
        float rangeSqr = range * range;

        if (sqr <= rangeSqr)
        {
            if (_attackRoutine == null)
                _attackRoutine = StartCoroutine(AttackLoop());
        }
        else
        {
            StopAttackRoutine();
        }
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            if (!Stats.IsAlive || _player == null || !_player.IsAlive)
            {
                _attackRoutine = null;
                yield break;
            }

            // 플레이어 쪽 바라보기
            Vector3 toPlayer = _player.Transform.position - transform.position;
            toPlayer.y = 0f;
            if (modelRoot != null && toPlayer != Vector3.zero)
                modelRoot.rotation = Quaternion.LookRotation(toPlayer);

            // 데미지
            _player.TakeDamage(Stats.AttackPower);

            // TODO: 공격 애니메이션, 사운드, 이펙트

            yield return new WaitForSeconds(Stats.AttackInterval);
        }
    }

    private void StopAttackRoutine()
    {
        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Stats == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Stats.AttackRange);
    }
}
