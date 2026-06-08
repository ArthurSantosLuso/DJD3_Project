using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [Tooltip("Audio to be played when the ability is performed.")]
    [SerializeField]
    protected AudioClip actionAudio;
    [SerializeField]
    protected float staminaCost;

    protected Character owner;
    protected Animator animator;


    private void Awake()
    {
        owner = GetComponentInParent<Character>()
        ?? GetComponent<Character>()
        ?? GetComponentInChildren<Character>();

        animator = owner.GetComponent<Animator>();
    }

    public virtual void Initialize(Character owner, Animator animator)
    {
        this.owner = owner;
        this.animator = animator;
    }

    public abstract float AbilityRange { get; }


    public abstract void Perform();
    protected abstract bool CanAttack();
    protected abstract void IdentifyEnemyInRange(List<IDamageable> entitiesHit);

    public virtual void EnableHitbox() { }
    public virtual void DisableHitbox() { }

    public virtual void DeployProjectile() { }
}
