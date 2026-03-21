using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyStats))]
public class EnemyController : MonoBehaviour
{
    public EnemyStats Stats { get; private set; }

    [Header("View")]
    public Transform modelRoot;

    [Header("AI")]
    [Tooltip("�Ÿ� üũ �ֱ� (��)")]
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
        // PlayerStats 대신 플레이어 오브젝트에 부착된 IAttackable(예: HealthSystem)을 가져옵니다.
        if (PlayerRef.Instance != null)
        {
            _player = PlayerRef.Instance.GetComponent<IAttackable>();
        }
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

            // �÷��̾� �� �ٶ󺸱�
            Vector3 toPlayer = _player.Transform.position - transform.position;
            toPlayer.y = 0f;
            if (modelRoot != null && toPlayer != Vector3.zero)
                modelRoot.rotation = Quaternion.LookRotation(toPlayer);

            // ������
            _player.TakeDamage(Stats.AttackPower);

            // TODO: ���� �ִϸ��̼�, ����, ����Ʈ

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
