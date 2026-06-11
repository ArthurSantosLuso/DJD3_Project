using System.Collections;
using UnityEngine;

public class BurningZone : MonoBehaviour
{
    private float damage;
    private float radius;
    private float duration;
    private float tickRate;
    private GameObject shooter;
    [SerializeField] private AudioClip startAudio;
    [SerializeField] private AudioClip endAudio;
    private AudioSource burningLoopSource;

    public void Initialize(float damage, float radius, float duration, float tickRate, GameObject shooter)
    {
        this.damage = damage;
        this.radius = radius;
        this.duration = duration;
        this.tickRate = tickRate;
        this.shooter = shooter;

        if(startAudio && AudioManager.Instance != null)
        burningLoopSource = AudioManager.Instance.PlayLoopingSound(startAudio);

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
                hit.GetComponent<IDamageable>()?.DamageNoStagger(damage);
            }

            yield return new WaitForSeconds(tickRate);
            elapsed += tickRate;
        }

        if (burningLoopSource != null)
        {
            burningLoopSource.Stop();
            burningLoopSource = null;
        }

        if (endAudio && AudioManager.Instance != null)
            AudioManager.Instance.PlaySound(endAudio);

        Destroy(gameObject);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}