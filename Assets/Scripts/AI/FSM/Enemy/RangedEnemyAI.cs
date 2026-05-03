using UnityEngine;

public class RangedEnemyAI : EnemyBaseAI
{
    protected override void Attack()
    {
        if (!isAttackState) return;

        timer += Time.deltaTime;

        if (timer >= timeToAttack)
        {
            GetComponent<Character>().UseAbility(0);
            timer = 0f;
        }
    }
}