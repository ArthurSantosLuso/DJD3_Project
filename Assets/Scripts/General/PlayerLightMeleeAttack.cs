using System.Collections.Generic;
using UnityEngine;

public class PlayerLightMeleeAttack : Ability
{
    [SerializeField] private float comboResetTime = 1.0f; // Time allowed between clicks

    private float lastAttackTime;
    private Animator anim;

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
            anim = owner.GetComponent<Animator>();
        }

    }

    public override float AbilityRange => throw new System.NotImplementedException();

    protected override bool CanAttack(Character character)
    {
        foreach (ValueBase valBase in character.ValueBases)
        {
            if (valBase is PlayerStamina)
            {
                if ((valBase as PlayerStamina).HasStamina(staminaCost))
                    return true;
            }
        }
        return false;
    }

    protected override void IdentifyEnemyInRange(List<IDamageable> entitiesHit)
    {
        throw new System.NotImplementedException();
    }

    public override void Perform(Character whoAttacked)
    {
        if (!CanAttack(whoAttacked))
            return;

        foreach (ValueBase valBase in whoAttacked.ValueBases)
        {
            if (valBase is PlayerStamina)
            {
                (valBase as PlayerStamina).UseStamina(staminaCost);
            }
        }

        if (owner.CharacterState == Character.State.Normal)
        {
            anim.SetBool("ComboSuccess", true);
            lastAttackTime = Time.time;
            owner.ChangeState(Character.State.Attacking);
        }
        else
        {
            float timeSinceLastAttack = Time.time - lastAttackTime;

            // Check if it should reset the combo based on time
            if (timeSinceLastAttack > comboResetTime)
            {
                anim.SetBool("ComboSuccess", false);
                return;
            }
            else
            {
                anim.SetBool("ComboSuccess", true);
            }
        }

        anim.SetTrigger("Attack");
        lastAttackTime = Time.time;
    }

    public void ResetComboState()
    {
        owner.ChangeState(Character.State.Normal);
    }

    public void StartComboState()
    {
        owner.ChangeState(Character.State.Attacking);
        anim.SetBool("ComboSuccess", false);
    }
}
