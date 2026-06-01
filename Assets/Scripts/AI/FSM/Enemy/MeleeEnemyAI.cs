using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeEnemyAI : EnemyBaseAI
{
    
    protected override void Attack()
    {
        if (!CheckIfCanProceed())
        {
            timer = 0;
            return;
        }

        timer += Time.deltaTime;

        if (timer >= timeToAttack)
        {
            transform.LookAt(target.transform);
            GetComponent<Character>().UseAbility(0);
            timer = 0f;
        }
    }
}
