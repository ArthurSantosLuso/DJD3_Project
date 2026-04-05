using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerLightMeleeAttack : Ability
{
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
        whoAttacked.PlayAnimation("Attack");
    }
}