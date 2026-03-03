using UnityEngine;

public class PlantMonsterFSM : MonoBehaviour
{
    // --- 1) 상위 생애 상태 ---
    public enum LifeState
    {
        Alive,
        DeadFlow
    }

    // --- 2) Alive 내부 전투/행동 상태(요구한 목록 그대로) ---
    public enum AliveState
    {
        Idle,
        Aggro,
        Hited,
        Attack,
        Special_Attack
    }

    // --- 3) DeadFlow 내부 상태(선언만, 지금은 미구현) ---
    public enum DeadState
    {
        Deading,
        Rewarding,
        Despawned,
        Respawning
    }

    [Header("Refs")]
    public BattlerBase self;
    public BattlerBase target;
    public Animator animator;


    [Header("Ranges (m)")]
    public float aggroRange = 6f;
    public float attackRange = 2.2f;

    [Header("Timings (sec)")]
    public float hitLockSec = 0.18f;
    public float attackWindupSec = 0.08f;
    public float cooldownSec = 0.65f;
    public float aggroForgetSec = 4.0f;

    [Header("Combat (0..1 normalized)")]
    [Range(0f, 1f)] public float damage01 = 0.06f;
    public float attackCdSec = 1.2f;

    [Header("Enrage (optional)")]
    [Range(0f, 1f)] public float enrageHpThreshold01 = 0.4f;
    public float enrageDamageMul = 1.4f;
    public float enrageCdMul = 0.8f;

    [Header("Debug")]
    public LifeState lifeState = LifeState.Alive;
    public AliveState aliveState = AliveState.Idle;
    public DeadState deadState = DeadState.Despawned;

    // ---- tick timers ----
    int _hitLockUntilTick = 0;
    int _pendingHitTick = -1;
    float _pendingHitDamage = 0f;

    int _nextAttackAllowedTick = 0;
    int _aggroLostTick = -1;
    bool _forcedAggro = false;

    void Awake()
    {
        if (self == null) self = GetComponent<BattlerBase>();
        if (animator == null) animator = GetComponent<Animator>();

    }

    private void Update()
    {
        if(aliveState == AliveState.Aggro)
            RotateToTargetYaw();

    }

    void RotateToTargetYaw(float maxDegPerSec = 720f)
    {
        if (self == null || target == null) return;

        Vector3 from = self.CombatTransform.position;
        Vector3 to = target.CombatTransform.position;

        Vector3 dir = to - from;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f) return;

        Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);

        if (maxDegPerSec == 0.0f)
            self.CombatTransform.rotation = look;
        else
        {
            // Tick 기반이니까 dt는 tick.DtSec을 쓰는 게 더 정석인데,
            // 지금은 간단히 Time.deltaTime 기반으로도 충분히 자연스럽게 보임.
            self.CombatTransform.rotation = Quaternion.Slerp(
                self.CombatTransform.rotation,
                look,
                10f * Time.deltaTime);
        }
    }




    /// <summary>
    /// 피격 이벤트 (Alive만 우선 처리)
    /// </summary>
    public void NotifyDamaged(CombatTick tick)
    {
        if (lifeState != LifeState.Alive) return;
        if (aliveState == AliveState.Idle) aliveState = AliveState.Aggro;

        _forcedAggro = true;

        int lockTicks = SecToTicks(hitLockSec, tick.DtSec);
        _hitLockUntilTick = Mathf.Max(_hitLockUntilTick, tick.Index + lockTicks);

        // 피격 상태로 잠깐 전환(시각적 반응)
        aliveState = AliveState.Hited;
    }

    public void OnTick(CombatTick tick)
    {
        // 0) 생애 상태 분기
        if (lifeState == LifeState.DeadFlow)
        {
            // TODO: DeadFlow 구현은 나중에
            return;
        }

        // 1) Alive만 우선 구현
        TickAlive(tick);
    }

    void TickAlive(CombatTick tick)
    {
        int t = tick.Index;

        // 사망 감지: 지금은 DeadFlow로 넘기기만(상세는 나중)
        if (self == null || !self.IsAlive)
        {
            lifeState = LifeState.DeadFlow;
            deadState = DeadState.Deading; // 선언만
            return;
        }

        // 타겟 유효성
        if (target == null || !target.IsAlive)
        {
            ResetAggro();
            aliveState = AliveState.Idle;
            return;
        }

        // Hit 락(경직) 중이면 아무 행동도 안 함
        if (t < _hitLockUntilTick) return;

        float dist = Vector3.Distance(self.CombatTransform.position, target.CombatTransform.position);
        bool inAggro = dist <= aggroRange || _forcedAggro;
        bool inAttack = dist <= attackRange;


        bool enraged = self.stats.hp01 <= enrageHpThreshold01;

        int windupTicks = SecToTicks(attackWindupSec, tick.DtSec);
        int cooldownTicks = SecToTicks(cooldownSec, tick.DtSec);
        int forgetTicks = SecToTicks(aggroForgetSec, tick.DtSec);

        float cdSec = attackCdSec * (enraged ? enrageCdMul : 1f);
        int attackCdTicks = SecToTicks(cdSec, tick.DtSec);

        float dmg = damage01 * (enraged ? enrageDamageMul : 1f);

        // 2) pending hit 처리(선딜 끝나면 데미지)
        if (_pendingHitTick >= 0 && t >= _pendingHitTick)
        {
            _pendingHitTick = -1;
            ApplyHit(_pendingHitDamage);
            aliveState = AliveState.Idle; // 일단 Idle로 두고 아래에서 Aggro로 다시 정리
        }

        // 3) 상태별 로직 (AliveState만)
        switch (aliveState)
        {
            case AliveState.Idle:
                {
                    if (inAggro)
                    {
                        aliveState = AliveState.Aggro;
                        if (animator != null)
                            animator.SetTrigger("Aggroed");
                    }
                    break;
                }

            case AliveState.Hited:
                {
                    // 피격 반응은 Hit 락으로 제어되고 있으니, 락이 끝났으면 Aggro로 복귀
                    aliveState = AliveState.Aggro;
                    break;
                }

            case AliveState.Aggro:
                {
                    // 어그로 유지/해제
                    if (!inAggro)
                    {
                        if (_aggroLostTick < 0) _aggroLostTick = t;
                        if (t - _aggroLostTick >= forgetTicks)
                        {
                            ResetAggro();
                            aliveState = AliveState.Idle;
                        }
                        break;
                    }
                    else
                    {
                        _aggroLostTick = -1;
                    }

                    // 공격 조건(특수공격은 나중에)
                    if (inAttack && t >= _nextAttackAllowedTick)
                    {
                        _nextAttackAllowedTick = t + attackCdTicks;

                        aliveState = AliveState.Attack;

                        // 선딜 처리: windupTicks==0이면 즉시, 아니면 pending으로
                        if (windupTicks <= 0)
                        {
                            ApplyHit(dmg);
                            aliveState = AliveState.Idle; // 아래에서 Aggro로 복귀
                            _hitLockUntilTick = Mathf.Max(_hitLockUntilTick, t + cooldownTicks);
                        }
                        else
                        {
                            _pendingHitTick = t + windupTicks;
                            _pendingHitDamage = dmg;

                            // 공격 모션 동안은 다른 행동 안 하게 약간 락(선딜)도 걸어줌
                            _hitLockUntilTick = Mathf.Max(_hitLockUntilTick, _pendingHitTick);
                        }
                    }
                    break;
                }

            case AliveState.Attack:
                {
                    // Attack은 트리거성 상태. 실제 제어는 락/펜딩이 해줌.
                    // 다음 틱에 Aggro로 정리
                    aliveState = AliveState.Aggro;
                    if (animator != null)
                        animator.SetTrigger("Aggroed");

                    break;
                }

            case AliveState.Special_Attack:
                {
                    // TODO: 나중에 구현
                    aliveState = AliveState.Aggro;
                    if (animator != null)
                        animator.SetTrigger("Aggroed");

                    break;
                }
        }

        // 4) 상태 정리: Idle로 떨어지지 않는 한 Aggro 유지가 자연스러움
        if (aliveState == AliveState.Idle) return;
        if (inAggro && aliveState != AliveState.Hited)
            aliveState = AliveState.Aggro;
    }

    void ApplyHit(float dmg01)
    {
        if (target == null || !target.IsAlive) return;
        target.ApplyDamage01(dmg01);
        // TODO: Hit FX/사운드/애니 이벤트
    }


    void ResetAggro()
    {
        _forcedAggro = false;
        _aggroLostTick = -1;
        _pendingHitTick = -1;
        _pendingHitDamage = 0f;

        animator.SetTrigger("Idle");
    }



    static int SecToTicks(float sec, float dt)
    {
        if (dt <= 0f) return 0;
        return Mathf.Max(0, Mathf.CeilToInt(sec / dt));
    }
}