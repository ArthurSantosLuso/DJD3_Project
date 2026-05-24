using System.Collections;
using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEngine;

public class PlayerLightMeleeAttack : Ability
{

    [Header("Blood FX")]
    [SerializeField] private GameObject bloodEffectPrefab;

    [Header("Combo Settings")]
    [SerializeField] private float comboResetTime = 1.0f;
    [SerializeField] private float damageAmount = 10f;

    [Header("Lunge Settings")]
    [Tooltip("Area where enemy is considered close range. trigger Normal attack")]
    [SerializeField] private float meleeRadius = 1.8f;

    [Tooltip("Area where the enemy is considered far. Triggers the lunge.")]
    [SerializeField] private float lungeRadius = 5f;

    [Tooltip("How long the lunge travel takes.")]
    [SerializeField] private float lungeDuration = 0.28f;

    [Tooltip("How high the player goes up during the lunge (Y units)")]
    [SerializeField] private float lungeArcHeight = 0.4f;

    [Tooltip("How far from the target the player stops.")]
    [SerializeField] private float lungeStopDistance = 0.9f;

    [Tooltip("Minimum distance to target required to trigger a lunge. Closer than this = normal attack.")]
    [SerializeField] private float minLungeDistance = 2.5f;

    [Tooltip("Seconds that must pass before the player can lunge again.")]
    [SerializeField] private float lungeCooldown = 2.5f;

    [Header("Enemy Scoring Weights")]
    [Tooltip("How much facing direction influences target selection.")]
    [SerializeField, Range(0f, 1f)] private float angleWeight = 0.65f;

    [Tooltip("How much proximity influences target selection.")]
    [SerializeField, Range(0f, 1f)] private float distanceWeight = 0.35f;

    [Header("Enemy Detection")]
    [Tooltip("Layer that contain enemies.")]
    [SerializeField] private LayerMask enemyLayer;


    private float               lastAttackTime;
    private float               lastLungeTime = -999f;
    private PlayerStamina       playerStamina;
    private Collider            hitboxCollider;
    private CharacterController characterController;
    private bool                isLunging;
    private List<IDamageable>   alreadyGotHit = new List<IDamageable>();

    public override float AbilityRange => throw new System.NotImplementedException();

    private void Start()
    {
        owner = GetComponentInParent<Character>()
            ?? GetComponent<Character>()
            ?? GetComponentInChildren<Character>();

        if (owner == null)
        {
            Debug.LogError($"[PlayerLightMeleeAttack] Could not find Character owner on {name}.");
            return;
        }

        animator = owner.GetComponent<Animator>();
        characterController = owner.GetComponent<CharacterController>();

        foreach (ValueBase valBase in owner.ValueBases)
        {
            if (valBase is PlayerStamina stamina)
            {
                playerStamina = stamina;
                break;
            }
        }

        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.enabled = false;
    }

    public override void EnableHitbox()
    {
        if (hitboxCollider != null)
            hitboxCollider.enabled = true;
    }

    public override void DisableHitbox()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
            alreadyGotHit.Clear();
        }
    }

    protected override bool CanAttack()
    {
        if (owner.CharacterState != Character.State.Normal &&
            owner.CharacterState != Character.State.Attacking)
            return false;

        return playerStamina.HasStamina(staminaCost);
    }

    public override void Perform()
    {
        if (!CanAttack()) return;

        if (owner.CharacterState == Character.State.Attacking)
        {
            HandleComboInput();
            return;
        }
        else
        {
            StartNormalAttack();
        }

        //Transform lungeTarget = FindBestLungeTarget();

        //if (lungeTarget != null)
        //    StartCoroutine(LungeRoutine(lungeTarget));
        //else
        //    StartNormalAttack();
    }


    private void StartNormalAttack()
    {
        animator.SetBool("ComboSuccess", true);
        animator.SetTrigger("Attack");
        lastAttackTime = Time.time;
        owner.ChangeState(Character.State.Attacking);
    }

    private void HandleComboInput()
    {
        float timeSinceLastAttack = Time.time - lastAttackTime;

        if (timeSinceLastAttack > comboResetTime)
        {
            animator.SetBool("ComboSuccess", false);
            return;
        }

        animator.SetBool("ComboSuccess", true);
        animator.SetTrigger("Attack");
        lastAttackTime = Time.time;
    }

    private Transform FindBestLungeTarget()
    {
        if (Time.time - lastLungeTime < lungeCooldown)
            return null;

        Collider[] hits = Physics.OverlapSphere(
            owner.transform.position, lungeRadius, enemyLayer);

        if (hits.Length == 0)
        {
            return null;
        }

        Transform bestTarget = null;
        float bestScore = -1f;

        foreach (Collider hit in hits)
        {
            if (hit.GetComponent<IDamageable>() == null) continue;

            float distance = Vector3.Distance(owner.transform.position, hit.transform.position);

            if (distance <= meleeRadius) continue;

            if (distance < minLungeDistance) continue;

            float score = ScoreTarget(hit.transform, distance);

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = hit.transform;
            }
        }

        return bestTarget;
    }

    private float ScoreTarget(Transform target, float distance)
    {
        Vector3 toTarget = (target.position - owner.transform.position).normalized;
        float angle = Vector3.Angle(owner.transform.forward, toTarget);

        float angleScore = 1f - (angle / 180f);
        float distanceScore = 1f - (distance / lungeRadius);

        return (angleScore * angleWeight) + (distanceScore * distanceWeight);
    }


    private IEnumerator LungeRoutine(Transform target)
    {
        isLunging = true;
        lastLungeTime = Time.time;
        owner.ChangeState(Character.State.Lunging);
        playerStamina.UseStamina(staminaCost);

        Vector3 flatDirection = target.position - owner.transform.position;
        flatDirection.y = 0f;
        if (flatDirection != Vector3.zero)
            owner.transform.rotation = Quaternion.LookRotation(flatDirection);

        animator.SetTrigger("Lunge");

        Vector3 startPos = owner.transform.position;
        Vector3 stopPoint = target.position - flatDirection.normalized * lungeStopDistance;
        stopPoint.y = startPos.y;

        float elapsed = 0f;

        while (elapsed < lungeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / lungeDuration);

            Vector3 desiredPos = Vector3.Lerp(startPos, stopPoint, t);
            desiredPos.y += lungeArcHeight * Mathf.Sin(t * Mathf.PI);

            Vector3 delta = desiredPos - owner.transform.position;
            characterController.Move(delta);

            yield return null;
        }

        isLunging = false;

        owner.ChangeState(Character.State.Normal);
        lastAttackTime = Time.time;
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable target = other.GetComponent<IDamageable>();
        IDamageable self = owner.gameObject.GetComponent<IDamageable>();

        if (target == null || target == self || alreadyGotHit.Contains(target)) return;
        if (!target.CanDamage()) return;

        target.Damage(damageAmount);
        alreadyGotHit.Add(target);

        Vector3 hitPoint = (transform.position + other.bounds.center) * 0.5f;
        SpawnBloodEffect(hitPoint);
    }

    private void SpawnBloodEffect(Vector3 position)
    {
        if (bloodEffectPrefab != null)
            Instantiate(bloodEffectPrefab, position, Quaternion.identity);
    }

    public void StartComboState()
    {
        owner.ChangeState(Character.State.Attacking);
        playerStamina.UseStamina(staminaCost);
        animator.SetBool("ComboSuccess", false);
    }

    public void ResetComboState()
    {
        owner.ChangeState(Character.State.Normal);
    }

    protected override void IdentifyEnemyInRange(List<IDamageable> entitiesHit)
    {
        throw new System.NotImplementedException();
    }
}