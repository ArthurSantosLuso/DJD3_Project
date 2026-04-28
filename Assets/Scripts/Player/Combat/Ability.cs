using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [SerializeField]
    protected float staminaCost;

    protected Character owner;
    protected Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public abstract float AbilityRange { get; }


    public abstract void Perform();
    protected abstract bool CanAttack();
    protected abstract void IdentifyEnemyInRange(List<IDamageable> entitiesHit);

    public virtual void EnableHitbox() { }
    public virtual void DisableHitbox() { }
}
