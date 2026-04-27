using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerLightMeleeAttack : Ability
{
    [SerializeField] private float comboResetTime = 1.0f; // Time allowed between clicks

    private float lastAttackTime;
    private PlayerStamina playerStamina;

    public override float AbilityRange => throw new System.NotImplementedException();

    private void Start()
    {
        owner = GetComponentInParent<Character>();

        if (owner == null)
        {
            owner = GetComponent<Character>();
        }

        if (owner == null)
        {
            owner = GetComponentInChildren<Character>();
        }

        if (owner == null)
        {
            Debug.Log($"Could not find owner for {this.name}");
        }
        else
        {
            animator = owner.GetComponent<Animator>();

            foreach (ValueBase valBase in owner.ValueBases)
            {
                if (valBase is PlayerStamina)
                {
                    playerStamina = valBase as PlayerStamina;
                }
            }
        }

    }


    protected override bool CanAttack()
    {
        // Check if player current state is different from normal
        if (owner.CharacterState != Character.State.Normal && owner.CharacterState != Character.State.Attacking)
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
    /// Reset the combo logic state 
    /// </summary>
    public void ResetComboState()
    {
        owner.ChangeState(Character.State.Normal);
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
}
