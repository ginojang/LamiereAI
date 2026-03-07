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

    [Header("Respawn Pos")]
    public Vector3 respawnPosition;

    [Header("Home Move")]
    public float returnSpeed = 0.1f;   // 초당 이동 속도
    public float maxChaseDistance = 3f; // 홈에서 최대 벗어날 거리


    [Header("Refs")]
    public BattlerBase self;
    public BattlerBase target;
    public Animator animator;


    [Header("Ranges (m)")]
    public float aggroRange = 6f;
    public float attackRange = 2.2f;
    public float minSeparation = 1.2f;

    [Header("Timings (sec)")]
    public float hitLockSec = 0.18f;
    public float attackHitSec = 0.08f;
    public float attackEndSec = 0.65f;
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

    int _aggroLostTick = -1;
    bool _forcedAggro = false;


    bool _isAttackStarted = false;
    bool _isAttackDamageApplied = false;
    bool _isMoveToRespawn = false;

    int _attackHitTick = -1;
    int _attackEndTick = -1;

    int _nextAttackAllowedTick = 0;


    [SerializeField]
    private string[] attackTriggers =
    {
        "Attack_1",
        "Attack_2",
        "Power_Attack"
    };



    void Awake()
    {
        if (self == null) self = GetComponent<BattlerBase>();
        if (animator == null) animator = GetComponent<Animator>();

        respawnPosition = transform.position;

    }

    private void Update()
    {
        if (aliveState == AliveState.Aggro)
        {
            RotateToTargetYaw();
            
        }
        else if(aliveState == AliveState.Idle)
        {
            MoveToRespawnPosition();
        }

    }

    void RotateToTargetYaw(float lerpSpeed = 10f)
    {
        if (self == null || target == null) return;

        Vector3 from = self.CombatTransform.position;
        Vector3 to = target.CombatTransform.position;

        Vector3 dir = to - from;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f) return;

        Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);

        if (lerpSpeed <= 0.0f)
            self.CombatTransform.rotation = look;
        else
        {
            // Tick 기반이니까 dt는 tick.DtSec을 쓰는 게 더 정석인데,
            // 지금은 간단히 Time.deltaTime 기반으로도 충분히 자연스럽게 보임.
            self.CombatTransform.rotation = Quaternion.Slerp(
                self.CombatTransform.rotation,
                look,
                lerpSpeed * Time.deltaTime);
        }
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

        // 사망 감지
        if (self == null || !self.IsAlive)
        {
            lifeState = LifeState.DeadFlow;
            deadState = DeadState.Deading;
            return;
        }

        // 타겟 유효성
        if (target == null || !target.IsAlive)
        {
            ResetAggro();
            aliveState = AliveState.Idle;
            return;
        }

        // Hit 락 중이면 아무 행동도 안 함
        //if (t < _hitLockUntilTick) return;

        float distanceTarget = Vector3.Distance(
            self.CombatTransform.position,
            target.CombatTransform.position);

        bool inAggro = distanceTarget <= aggroRange || _forcedAggro;

        int forgetTicks = SecToTicks(aggroForgetSec, tick.DtSec);

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
                    // 피격 반응 끝나면 Aggro로 복귀
                    aliveState = AliveState.Aggro;
                    break;
                }

            case AliveState.Aggro:
                {
                    if (CheckAttack(tick, distanceTarget))
                        break;

                    if (!inAggro)
                    {
                        if (_aggroLostTick < 0) _aggroLostTick = t;

                        if (t - _aggroLostTick >= forgetTicks)
                        {
                            ResetAggro();
                            aliveState = AliveState.Idle;
                        }
                    }
                    else
                    {
                        _aggroLostTick = -1;
                    }

                    break;
                }

            case AliveState.Attack:
                {
                    LoopAttack(tick, distanceTarget);
                    break;
                }

            case AliveState.Special_Attack:
                {
                    // 나중에 LoopSpecialAttack(tick) 구조로 가면 됨
                    break;
                }
        }
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

        _attackHitTick = -1;
        _attackEndTick = -1;

        animator.SetTrigger("Idle");
    }


    bool CheckAttack(CombatTick tick, float distanceTarget)
    {
        bool inAttack = distanceTarget <= attackRange;
        int t = tick.Index;

        if (!inAttack) return false;
        if (t < _nextAttackAllowedTick) return false;

        StartAttack(tick);
        return true;
    }

    void StartAttack(CombatTick tick)
    {
        int t = tick.Index;

        aliveState = AliveState.Attack;

        _isAttackStarted = true;
        _isAttackDamageApplied = false;
        _isMoveToRespawn = false;

        _attackHitTick = t + SecToTicks(attackHitSec, tick.DtSec);
        _attackEndTick = t + SecToTicks(attackEndSec, tick.DtSec);

        if (animator != null && attackTriggers != null && attackTriggers.Length > 0)
        {
            int rand = Random.Range(0, attackTriggers.Length);
            animator.SetTrigger(attackTriggers[rand]);
        }
    }

    void LoopAttack(CombatTick tick, float distanceTarget)
    {
        int t = tick.Index;

        if (!_isAttackStarted)
        {
            aliveState = AliveState.Aggro;
            return;
        }

        // 타겟 유효성 체크
        if (target == null || !target.IsAlive)
        {
            EndAttack();
            ResetAggro();
            aliveState = AliveState.Idle;
            return;
        }

        // Enrage 계산
        bool enraged = self.stats.hp01 <= enrageHpThreshold01;
        float dmg = damage01 * (enraged ? enrageDamageMul : 1f);

        float cdSec = attackCdSec * (enraged ? enrageCdMul : 1f);
        int attackCdTicks = SecToTicks(cdSec, tick.DtSec);

        // 1. 데미지 주는 시점
        if (!_isAttackDamageApplied && t >= _attackHitTick)
        {
            _isAttackDamageApplied = true;

            if (distanceTarget <= attackRange)
            {
                ApplyHit(dmg);
            }
        }

        // 2. 공격 종료
        if (t >= _attackEndTick)
        {
            _nextAttackAllowedTick = t + attackCdTicks;
            EndAttack();
            aliveState = AliveState.Aggro;
        }

        if (distanceTarget < minSeparation)
        {
            ResolveMinSeparationSmooth(
                self.CombatTransform,
                target.CombatTransform,
                minSeparation
            );
        }
    }

    void EndAttack()
    {
        _isAttackStarted = false;
        _isAttackDamageApplied = false;
        _isMoveToRespawn = false;
        _attackHitTick = -1;
        _attackEndTick = -1;
    }


    void ResolveMinSeparationSmooth(Transform mover, Transform anchor, float minSeparation, float moveLerp = 30f)
    {
        if (mover == null || anchor == null) return;

        Vector3 moverPos = mover.position;
        Vector3 anchorPos = anchor.position;

        Vector3 delta = moverPos - anchorPos;
        delta.y = 0f;

        float dist = delta.magnitude;
        if (dist <= 0.0001f) return;

        if (dist < minSeparation)
        {
            Vector3 desired = anchorPos + delta.normalized * minSeparation;
            Vector3 next = Vector3.Lerp(
                moverPos,
                new Vector3(desired.x, moverPos.y, desired.z),
                moveLerp * Time.deltaTime);

            mover.position = next;
        }
    }


    void MoveToRespawnPosition()
    {
        if (self == null) return;

        Transform tr = self.CombatTransform;

        Vector3 pos = tr.position;
        Vector3 target = respawnPosition;

        target.y = pos.y; // 수평 이동만

        Vector3 delta = target - pos;
        float dist = delta.magnitude;

        if (dist <= 0.05f)
        {
            return;
        }


        // 첫 이동 시작할떄. 애니메이션 준다.
        if (animator != null && _isMoveToRespawn == false)
            animator.SetTrigger("Attack_1");

        _isMoveToRespawn = true;

        Vector3 move = delta.normalized * returnSpeed * Time.deltaTime;
        if (move.magnitude > dist)
            move = delta;

        tr.position += move;
    }


    static int SecToTicks(float sec, float dt)
    {
        if (dt <= 0f) return 0;
        return Mathf.Max(0, Mathf.CeilToInt(sec / dt));
    }
}