using UnityEngine;

public class RangedEnemyAI : EnemyBaseAI
{
    protected override void Attack()
    {
        if (isAttackState)
        {
            timer += Time.deltaTime;

            if (timer >= timeToAttack)
            {
                transform.LookAt(target.transform);
                //Debug.Log("Ranged Attack!");
                //GetComponent<Character>().UseAbility(0);
                timer = 0f;
            }
        }
    }
}
