using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [Tooltip("Audio to be played when the ability is performed.")]
    [SerializeField]
    protected AudioClip actionAudio;
    [SerializeField]
    protected float staminaCost;

    [Header("On hit effects propreties")]
    [SerializeField]
    protected float shakeIntensity = 0.5f;
    [SerializeField]
    protected float shakeDuration = 0.2f;
    [SerializeField]
    protected float shakeDelay = 0.2f;
    [SerializeField]
    protected float onHitTimeScale = 0.02f;
    [SerializeField]
    protected float onHitTimeScaleDuration = 0.05f;

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

    protected void ImpactFrame()
    {

    }

    public abstract void Perform();
    protected abstract bool CanAttack();
    protected abstract void IdentifyEnemyInRange(List<IDamageable> entitiesHit);

    public virtual void EnableHitbox() { }
    public virtual void DisableHitbox() { }

    public virtual void DeployProjectile() { }
}
