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

    [Header("Enemy Detection")]
    [Tooltip("Layer that contain enemies.")]
    [SerializeField] private LayerMask enemyLayer;


    private float               lastAttackTime;
    private PlayerStamina       playerStamina;
    private Collider            hitboxCollider;
    private CharacterController characterController;
    // private bool                isLunging;
    private List<IDamageable>   alreadyGotHit = new List<IDamageable>();

    public override float AbilityRange => throw new System.NotImplementedException();

    public override void Initialize(Character owner, Animator animator)
    {
        base.Initialize(owner, animator);

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
        if (!enabled) return false;
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

    private void OnTriggerEnter(Collider other)
    {
        IDamageable target = other.GetComponent<IDamageable>();
        Debug.Log(other.gameObject.name);
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