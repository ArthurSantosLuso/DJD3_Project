using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    protected Character owner;
    protected Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public abstract float AbilityRange { get; }

    [SerializeField]
    protected float staminaCost;

    public abstract void Perform();
    protected abstract bool CanAttack();
    protected abstract void IdentifyEnemyInRange(List<IDamageable> entitiesHit);
}
