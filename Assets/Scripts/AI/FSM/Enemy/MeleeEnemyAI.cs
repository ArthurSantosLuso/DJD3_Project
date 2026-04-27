using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeEnemyAI : EnemyBaseAI
{
    protected override void Attack()
    {
        if (isAttacking)
        {
            timer += Time.deltaTime;

            if (timer >= timeToAttack)
            {
                GetComponent<Character>().UseAbility(0);
                timer = 0f;
            }
        }
    }
}
