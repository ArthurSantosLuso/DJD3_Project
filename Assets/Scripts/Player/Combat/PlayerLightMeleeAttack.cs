using System.Collections.Generic;
using UnityEngine;

public class PlayerLightMeleeAttack : Ability
{
    [SerializeField] private GameObject bloodEffectPrefab;
    [SerializeField] private float comboResetTime = 1.0f; // Time allowed between clicks
    [SerializeField] private float damageAmout = 10f;

    private float lastAttackTime;
    private PlayerStamina playerStamina;
    private Collider hitboxCollider;

    public override float AbilityRange => throw new System.NotImplementedException();



    private void Start()
    {
        owner = GetComponentInParent<Character>()
            ?? GetComponent<Character>()
            ?? GetComponentInChildren<Character>();

        if (owner == null)
        {
            Debug.Log($"Could not find owner for {this.name}");
            return;
        }

        animator = owner.GetComponent<Animator>();

        foreach (ValueBase valBase in owner.ValueBases)
        {
            if (valBase is PlayerStamina)
            {
                playerStamina = valBase as PlayerStamina;
                break;
            }
        }

        hitboxCollider = GetComponent<Collider>();
        // Avoid unnecessary collision detection
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
            hitboxCollider.enabled = false;
    }

    protected override bool CanAttack()
    {
        // Check if player current state is different from normal
        if (owner.CharacterState != Character.State.Normal &&
            owner.CharacterState != Character.State.Attacking)
            return false;

        // Check if player has enough stamina to attack
        if (playerStamina.HasStamina(staminaCost))
            return true;
        else return false;
    }

    protected override void IdentifyEnemyInRange(List<IDamageable> entitiesHit)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Perform the ability
    /// </summary>
    public override void Perform()
    {
        // Check if can attack
        if (!CanAttack())
            return;

        // Check if it's the first attack
        if (owner.CharacterState == Character.State.Normal)
        {
            animator.SetBool("ComboSuccess", true);
            lastAttackTime = Time.time;
            owner.ChangeState(Character.State.Attacking);
        }
        else
        {
            // Store the time between the current time and the time of the last attack
            float timeSinceLastAttack = Time.time - lastAttackTime;

            // Check if it should reset the combo based on combo window time
            if (timeSinceLastAttack > comboResetTime)
            {
                animator.SetBool("ComboSuccess", false);
                return;
            }
            else
            {
                animator.SetBool("ComboSuccess", true);
            }
        }

        animator.SetTrigger("Attack");
        // Store the time of the last attack
        lastAttackTime = Time.time;
    }

    /// <summary>
    /// Start the combo logic state
    /// </summary>
    public void StartComboState()
    {
        owner.ChangeState(Character.State.Attacking);
        // Use the stamina
        playerStamina.UseStamina(staminaCost);
        animator.SetBool("ComboSuccess", false);
    }

    /// <summary>
    /// Reset the combo logic state 
    /// </summary>
    public void ResetComboState()
    {
        owner.ChangeState(Character.State.Normal);
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable target = other.GetComponent<IDamageable>();
        IDamageable self = owner.gameObject.GetComponent<IDamageable>();

        if (target == null || target == self) return;

        if (target.CanDamage())
            target.Damage(damageAmout);

        Vector3 hitPoint = (transform.position + other.bounds.center) * 0.5f;
        SpawnBloodEffect(hitPoint);

        // Prevent multi-hit
        DisableHitbox(); 
    }

    private void SpawnBloodEffect(Vector3 position)
    {
        if (bloodEffectPrefab != null)
            Instantiate(bloodEffectPrefab, position, Quaternion.identity);
    }


}
