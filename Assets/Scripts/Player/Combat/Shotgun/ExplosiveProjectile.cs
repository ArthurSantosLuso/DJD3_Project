using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ExplosiveProjectile : MonoBehaviour
{
    private float impactDamage;
    private float burnDamage;
    private float burnRadius;
    private float burnDuration;
    private float burnTickRate;
    private GameObject shooter;
    private GameObject burningZonePrefab;
    private Vector3 direction;
    private float speed;

    private bool hasHit = false;

    public void Initialize(float impactDamage, float burnDamage, float burnRadius, float burnDuration, float burnTickRate, Vector3 direction, float speed, GameObject shooter, GameObject burningZonePrefab)
    {
        this.impactDamage = impactDamage;
        this.burnDamage = burnDamage;
        this.burnRadius = burnRadius;
        this.burnDuration = burnDuration;
        this.burnTickRate = burnTickRate;
        this.direction = direction.normalized;
        this.speed = speed;
        this.shooter = shooter;
        this.burningZonePrefab = burningZonePrefab;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        if (hasHit) return;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        if (other.gameObject == shooter) return;
        if (other.GetComponent<ExplosiveProjectile>() != null) return;

        hasHit = true;

        other.GetComponent<IDamageable>()?.Damage(impactDamage);

        Vector3 spawnPos = transform.position;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10f))
            spawnPos = hit.point;

        GameObject zone = Instantiate(burningZonePrefab, spawnPos, Quaternion.identity);

        zone.GetComponent<BurningZone>().Initialize(burnDamage, burnRadius, burnDuration, burnTickRate, shooter);

        Destroy(gameObject);
    }
}