using UnityEngine;


[System.Serializable]
public class BattlerStats
{
    [Range(0f, 1f)] public float hp01 = 1f;
    [Range(0f, 1f)] public float atk01 = 0.08f; // 한 번에 깎는 정규화 데미지(테스트용)
    public float attackRange = 2.2f;
    public float attackCd = 1.2f;

}


public abstract class BattlerBase : MonoBehaviour, IBattler
{
    public BattlerStats stats = new BattlerStats();
    public float LastAttackTime { get; set; } = -999f;

    public bool IsAlive => stats.hp01 > 0.0001f;
    public float Hp01 => stats.hp01;
    public float AttackRange => stats.attackRange;
    public float AttackCd => stats.attackCd;
    public virtual Transform CombatTransform => transform;

    public virtual void ApplyDamage01(float dmg01)
    {
        if (!IsAlive) return;
        stats.hp01 = Mathf.Clamp01(stats.hp01 - Mathf.Max(0f, dmg01));
        if (!IsAlive)
        {
            // TODO: 죽음 애니/콜라이더 off 등
            // Debug.Log($"{name} died");
        }
    }

}

public interface IBattler
{
    bool IsAlive { get; }
    float Hp01 { get; }          // 0..1
    float AttackRange { get; }   // meters
    float AttackCd { get; }      // seconds
    float LastAttackTime { get; set; }

    UnityEngine.Transform CombatTransform { get; }

    void ApplyDamage01(float dmg01); // 정규화 데미지
}