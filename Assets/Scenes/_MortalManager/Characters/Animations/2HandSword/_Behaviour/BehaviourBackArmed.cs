using UnityEngine;


public class BehaviourBackArmed : StateMachineBehaviour
{
    [SerializeField] private string onlyStateName = "Armed";

    private int _hash;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_hash == 0) _hash = Animator.StringToHash(onlyStateName);
        if (stateInfo.shortNameHash != _hash) return;

        //if (!string.IsNullOrEmpty(onlyStateName) && !stateInfo.IsName(onlyStateName)) return;

        animator.GetComponentInParent<MyCharacterCore>()?.OnBackArmedAnimationStart();

    }

}

