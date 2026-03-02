using UnityEngine;

public class CombatResolver
{
    public void TryAutoAttack(IBattler attacker, IBattler target, float nowTime, float atk01)
    {
        if (attacker == null || target == null) return;
        if (!attacker.IsAlive || !target.IsAlive) return;

        // 쿨타임
        if (nowTime - attacker.LastAttackTime < attacker.AttackCd) return;

        // 거리
        float dist = Vector3.Distance(attacker.Transform.position, target.Transform.position);
        if (dist > attacker.AttackRange) return;

        // 공격 성립
        attacker.LastAttackTime = nowTime;
        target.ApplyDamage01(atk01);

        // TODO: 이벤트(피격/공격) 발생 → 애니/FX는 여기서 분리
    }
}