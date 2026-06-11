using System;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangAxe : MonoBehaviour
{
    [SerializeField] private float maxTravelDistance = 10f;

    private GameObject shooter;
    private Transform returnTarget;
    private Transform homingTarget;
    private Vector3 direction;
    private float speed;
    private float damage;
    private Action onReturned;
    private Vector3 startPosition;

    private AudioClip hitSound;
    private Vector2 hitPitchRange;

    private enum Phase { GoingOut, Returning }
    private Phase phase = Phase.GoingOut;

    private float returnDistance = 0.5f;
    private List<IDamageable> alreadyHit = new List<IDamageable>();

    public void Initialize(GameObject shooter, Transform returnTarget, Transform homingTarget, Vector3 direction, float speed, float damage, Action onReturned, AudioClip hitSound, Vector2 hitPitchRange)
    {
        this.shooter = shooter;
        this.returnTarget = returnTarget;
        this.homingTarget = homingTarget;
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;
        this.onReturned = onReturned;
        this.hitSound = hitSound;
        this.hitPitchRange = hitPitchRange;
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.forward, 720f * Time.deltaTime, Space.Self);

        if (phase == Phase.GoingOut)
            HandleGoingOut();
        else
            HandleReturning();
    }

    private void HandleGoingOut()
    {
        if (homingTarget != null)
            direction = Vector3.Lerp(direction, (homingTarget.position - transform.position).normalized, 8f * Time.deltaTime);

        transform.position += direction * speed * Time.deltaTime;

        bool reachedTarget = homingTarget != null && Vector3.Distance(transform.position, homingTarget.position) < 0.5f;
        bool reachedMaxDistance = Vector3.Distance(transform.position, startPosition) >= maxTravelDistance;

        if (reachedTarget || reachedMaxDistance)
        {
            phase = Phase.Returning;
            alreadyHit.Clear();
        }
    }

    private void HandleReturning()
    {
        Vector3 toPlayer = (returnTarget.position - transform.position).normalized;
        transform.position += toPlayer * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, returnTarget.position) < returnDistance)
        {
            onReturned?.Invoke();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == shooter) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable == null) return;
        if (!damageable.CanDamage()) return;
        if (alreadyHit.Contains(damageable)) return;

        damageable.Damage(damage);
        alreadyHit.Add(damageable);

        if (hitSound && AudioManager.Instance != null)
            AudioManager.Instance.PlaySound(hitSound, UnityEngine.Random.Range(hitPitchRange.x, hitPitchRange.y));
    }
}