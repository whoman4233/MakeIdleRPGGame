using System.Collections;
using UnityEngine;

public class AttackState : IPlayerState
{
    private PlayerController _c;
    private MonoBehaviour _host;
    private Coroutine _attackRoutine;

    public void Enter(PlayerController controller)
    {
        _c = controller;
        _host = controller;
        StartAttackLoop();
    }

    public void Tick()
{
    if (_c.CurrentTarget == null || !_c.CurrentTarget.IsAlive)
    {
        _c.ChangeState(PlayerStateType.MoveForward); // 타겟 없으면 헛스윙(대기)
        return;
    }

    float dist = _c.DistanceToTarget();
    if (dist > _c.attackRange * 1.2f)
    {
        // 런닝머신이므로 Chase(추적)하지 않고, 다시 MoveForward(대기)로 돌아가서 다가오길 기다림
        _c.ChangeState(PlayerStateType.MoveForward); 
    }
}

    public void Exit()
    {
        if (_attackRoutine != null && _host != null)
        {
            _host.StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }
    }

    private void StartAttackLoop()
    {
        if (_host == null) return;

        if (_attackRoutine != null)
            _host.StopCoroutine(_attackRoutine);

        _attackRoutine = _host.StartCoroutine(AttackLoop());
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            if (_c.CurrentTarget == null || !_c.CurrentTarget.IsAlive)
            {
                _c.ChangeState(PlayerStateType.MoveForward);
                yield break;
            }

            Vector3 toTarget = _c.CurrentTarget.Transform.position - _c.transform.position;
            
            // 2.5D 보정: Z축, Y축 무시
            toTarget.y = 0f;
            toTarget.z = 0f; 

            if (_c.modelRoot != null && toTarget != Vector3.zero)
                _c.modelRoot.rotation = Quaternion.LookRotation(toTarget);

            // [변경점] AttackState 내부에서 직접 스탯을 읽어와서 공격을 실행합니다.
            // Stats.GetStat() 또는 GetStatValue() 등 선언하신 메서드명에 맞게 호출
            float finalDamage = _c.Stats.GetStatValue(StatType.AttackPower);
            _c.CurrentTarget.TakeDamage(finalDamage);
            
            Debug.Log($"[AttackState] 적에게 {finalDamage} 데미지 타격!");

            // 공격 속도 로직 (메서드명 주의)
            float currentAttackSpeed = _c.Stats.GetStatValue(StatType.AttackSpeed);
            float attackInterval = 1f / Mathf.Max(0.1f, currentAttackSpeed);

            yield return new WaitForSeconds(attackInterval);
        }
    }
}