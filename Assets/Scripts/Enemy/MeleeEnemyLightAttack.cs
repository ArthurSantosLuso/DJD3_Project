using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemyLightAttack : Ability
{
    public override float AbilityRange => throw new System.NotImplementedException();

    public override void Perform()
    {
        Debug.Log($"{this.name} attacked the player.");
    }

    protected override bool CanAttack()
    {
        throw new System.NotImplementedException();
    }

    protected override void IdentifyEnemyInRange(List<IDamageable> entitiesHit)
    {
        throw new System.NotImplementedException();
    }
}
