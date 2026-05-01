using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemyLightAttack : Ability
{
    [SerializeField] private float damageAmount = 10f;

    public override float AbilityRange => throw new System.NotImplementedException();

    private void Start()
    {
        owner = GetComponentInParent<Character>()
            ?? GetComponent<Character>()
            ?? GetComponentInChildren<Character>();
    }

    public override void Perform()
    {
        owner.PlayAnimation("Attack");
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable target = other.GetComponent<IDamageable>();
        //IDamageable self = owner.gameObject.GetComponent<IDamageable>();

        //if (target == null || target == self) return;
        //if (!target.CanDamage()) return;
        if (target is PlayerHealth)
        {
            target.Damage(damageAmount);
            DisableHitbox();
        }
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
