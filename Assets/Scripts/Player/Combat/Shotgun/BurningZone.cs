using System.Collections;
using UnityEngine;

public class BurningZone : MonoBehaviour
{
    private float damage;
    private float radius;
    private float duration;
    private float tickRate;
    private GameObject shooter;

    public void Initialize(float damage, float radius, float duration, float tickRate, GameObject shooter)
    {
        this.damage = damage;
        this.radius = radius;
        this.duration = duration;
        this.tickRate = tickRate;
        this.shooter = shooter;

        StartCoroutine(BurnRoutine());
    }

    private IEnumerator BurnRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider hit in hits)
            {
                if (hit.gameObject == shooter) continue;
                hit.GetComponent<IDamageable>()?.Damage(damage);
            }

            yield return new WaitForSeconds(tickRate);
            elapsed += tickRate;
        }

        Destroy(gameObject);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}