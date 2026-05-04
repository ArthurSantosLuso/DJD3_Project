using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemyLightAttack : Ability
{
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private Collider hitbox; // assign in Inspector

    public override float AbilityRange => 0f; // fill in if needed

    private void Start()
    {
        owner = GetComponentInParent<Character>()
            ?? GetComponent<Character>()
            ?? GetComponentInChildren<Character>();

        // Ensure hitbox starts disabled
        if (hitbox != null)
            hitbox.enabled = false;
    }

    public override void Perform()
    {
        owner.PlayAnimation("Attack");
        // EnableHitbox() will be called by the animation event at the right frame
    }

    // Called by Animation Event
    public override void EnableHitbox()
    {
        if (hitbox != null)
            hitbox.enabled = true;
    }

    // Called by Animation Event (at end of attack swing)
    public override void DisableHitbox()
    {
        if (hitbox != null)
            hitbox.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable target = other.GetComponent<IDamageable>();

        if (target is PlayerHealth)
        {
            target.Damage(damageAmount);
            DisableHitbox(); // prevent multi-hit in the same swing
        }
    }

    protected override bool CanAttack() => true;

    protected override void IdentifyEnemyInRange(List<IDamageable> entitiesHit) { }
}