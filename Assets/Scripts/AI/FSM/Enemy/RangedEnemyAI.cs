using UnityEngine;

public class RangedEnemyAI : EnemyBaseAI
{
    [SerializeField, Min(1f)]
    private float rotationSpeed = 8f;

    protected override void Attack()
    {
        // Rotate the enemy to face the player before/while shooting.
        if (target != null)
        {
            Vector3 dirToPlayer = (target.transform.position - transform.position);
            dirToPlayer.y = 0f;
            if (dirToPlayer.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(dirToPlayer);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime);
            }
        }

        // Keep the flee delay ticking even while the enemy is in attack state.
        TickFleeTimer();

        timer += Time.deltaTime;

        if (timer >= timeToAttack)
        {
            GetComponent<Character>().UseAbility(0);
            timer = 0f;
        }
    }
}