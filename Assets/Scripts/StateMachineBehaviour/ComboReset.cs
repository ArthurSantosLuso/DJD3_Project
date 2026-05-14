using UnityEngine;

public class ComboReset : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerLightMeleeAttack attack = animator.GetComponentInChildren<PlayerLightMeleeAttack>();
        if (attack != null)
        {
            attack.StartComboState();
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerLightMeleeAttack attack = animator.GetComponentInChildren<PlayerLightMeleeAttack>();
        if (attack != null)
        {
            attack.ResetComboState();
        }
    }
}
