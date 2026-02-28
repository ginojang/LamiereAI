using UnityEngine;

public class BehaviourNormalAttack3 : StateMachineBehaviour
{
    [Header("Filter")]
    [SerializeField] private string onlyStateName = "NormalAttack_3";

    [Header("Timings (normalized 0~1)")]
    [Range(0f, 1f)] public float slashStartN = 0.25f;   // 트레일/스윙 시작
    [Range(0f, 1f)] public float hitN = 0.35f;          // 히트(스파크/사운드)
    [Range(0f, 1f)] public float slashEndN = 0.75f;     // 트레일 종료

    private bool _firedStart;
    private bool _firedHit;
    private bool _firedEnd;


    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!IsTargetState(stateInfo)) return;

        _firedStart = false;
        _firedHit = false;
        _firedEnd = false;

        // 진입 즉시 코어에게 알려도 됨 (원하면)
        animator.GetComponentInParent<MyCharacterCore>()?.OnNormalAttackEnter(3);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!IsTargetState(stateInfo)) return;

        var core = animator.GetComponentInParent<MyCharacterCore>();
        if (core == null) return;

        float n = GetNormalized01(stateInfo);

        if (!_firedStart && n >= slashStartN)
        {
            _firedStart = true;
            core.OnSwordSlashStart(3);
        }

        if (!_firedHit && n >= hitN)
        {
            _firedHit = true;
            core.OnSwordSlashHit(3);
        }

        if (!_firedEnd && n >= slashEndN)
        {
            _firedEnd = true;
            core.OnSwordSlashEnd(3);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!IsTargetState(stateInfo)) return;

        animator.GetComponentInParent<MyCharacterCore>()?.OnNormalAttackExit(3);
    }

    private bool IsTargetState(AnimatorStateInfo info)
    {
        // stateInfo.IsName()은 "Base Layer.NormalAttack_1" 같은 풀패스일 수도 있어서,
        // 네 프로젝트에 맞게 onlyStateName을 풀패스로 넣거나, 아래처럼 둘 다 허용.
        if (string.IsNullOrEmpty(onlyStateName)) return true;
        return info.IsName(onlyStateName) || info.IsName($"Base Layer.{onlyStateName}");
    }

    private float GetNormalized01(AnimatorStateInfo info)
    {
        float t = info.normalizedTime;
        return t - Mathf.Floor(t);
    }
}