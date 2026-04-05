using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class Ability : MonoBehaviour
{

    public abstract float AbilityRange { get; }

    [SerializeField]
    protected float staminaCost;

    public abstract void Perform(Character whoAttacked);
    protected abstract bool CanAttack(Character whoAttacked);
    protected abstract void IdentifyEnemyInRange(List<IDamageable> entitiesHit);
}
