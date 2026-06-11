using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private AudioClip hitObject;
    [SerializeField] private AudioClip hitEnemy;

    private Vector3 direction;
    private float speed;
    private float damage;
    private GameObject shooter;
    private GameObject bloodEffectPrefab;
    private bool hasHit = false;

    public void Initialize(Vector3 direction, float speed, float damage, GameObject shooter, GameObject bloodEffectPrefab = null)
    {
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;
        this.shooter = shooter;
        this.bloodEffectPrefab = bloodEffectPrefab;

        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Collider>().isTrigger = true;

        Destroy(gameObject, lifetime);
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
        if (other.GetComponent<Bullet>() != null) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        
        if(damageable != null)
        {
            AudioManager.Instance.PlaySound(hitEnemy);
            damageable.Damage(damage);

            if (damageable != null && bloodEffectPrefab != null)
            {
                Instantiate(bloodEffectPrefab, other.ClosestPoint(transform.position), Quaternion.identity);
            }
        }
        else
        {
            AudioManager.Instance.PlaySound(hitObject, 0.5f);
        }

        hasHit = true;
        Debug.Log($"Acertei isso: {other.gameObject.name}");
        Destroy(gameObject);
    }
}