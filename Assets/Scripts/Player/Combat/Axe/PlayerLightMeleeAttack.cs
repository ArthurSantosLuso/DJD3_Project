using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerLightMeleeAttack : Ability
{

    [Header("VFX")]
    [SerializeField] private GameObject bloodEffectPrefab;
    [SerializeField] private ParticleSystem axeParticles;

    [Header("Combo Settings")]
    [SerializeField] private float comboResetTime = 1.0f;
    [SerializeField] private float damageAmount = 10f;

    [Header("Enemy Detection")]
    [Tooltip("Layer that contain enemies.")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Audio")]
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);
    [SerializeField] private AudioClip hitSound;

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
        axeParticles.Stop();

        List<UpgradeData> upgrades = GameManager.Instance.PlayerUpgrades
            .Where(s => s.UpgradeType == UpgradeData.UpgradeTypes.AxeDamage)
            .ToList();

        foreach (UpgradeData upgrade in upgrades)
        {
            damageAmount += upgrade.AmountToChange;
        }
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
        IDamageable self = owner.gameObject.GetComponent<IDamageable>();

        if (target == null || target == self || alreadyGotHit.Contains(target)) return;
        if (!target.CanDamage()) return;

        target.Damage(damageAmount);
        alreadyGotHit.Add(target);

        StartCoroutine(OnHitEffects());

        Vector3 hitPoint = (transform.position + other.bounds.center) * 0.5f;
        if(target.HasBlood()) SpawnBloodEffect(hitPoint);

        if (hitSound && AudioManager.Instance != null)
            AudioManager.Instance.PlaySound(hitSound, Random.Range(pitchRange.x, pitchRange.y));
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
        axeParticles.Play();
        if (attackSounds.Length > 0)
            AudioManager.Instance.PlaySound(
                attackSounds[Random.Range(0, attackSounds.Length)],
                Random.Range(pitchRange.x, pitchRange.y)
            );
    }

    public void ResetComboState()
    {
        if (owner.CharacterState == Character.State.UsingAbility) return;
        owner.ChangeState(Character.State.Normal);
        axeParticles.Stop();
    }
    private void OnEnable()
    {
        axeParticles.Stop();
    }

    private void OnDisable()
    {
        axeParticles.Stop();
        if (owner.CharacterState != Character.State.UsingAbility)
            owner.ChangeState(Character.State.Normal);
    }

    protected override void IdentifyEnemyInRange(List<IDamageable> entitiesHit)
    {
        throw new System.NotImplementedException();
    }
}