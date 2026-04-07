using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class Ability : MonoBehaviour
{
    protected Character owner;

    public abstract float AbilityRange { get; }

    [SerializeField]
    protected float staminaCost;

    public abstract void Perform();
    protected abstract bool CanAttack();
    protected abstract void IdentifyEnemyInRange(List<IDamageable> entitiesHit);
}
