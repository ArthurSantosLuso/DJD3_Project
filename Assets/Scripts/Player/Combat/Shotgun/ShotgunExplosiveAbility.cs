using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShotgunExplosiveAbility : Ability
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject explosiveProjectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 25f;
    [SerializeField] private float impactDamage = 60f;

    [Header("Burning Zone Settings")]
    [SerializeField] private GameObject burningZonePrefab;
    [SerializeField] private float burnDamage = 5f;
    [SerializeField] private float burnRadius = 3f;
    [SerializeField] private float burnDuration = 4f;
    [SerializeField] private float burnTickRate = 0.5f;

    [Header("Ability Settings")]
    [SerializeField] private float cooldown = 5f;
    private const int ammoCost = 2;

    public float CooldownProgress => Mathf.Clamp01((Time.time - lastUseTime) / cooldown);
    public bool IsReady => Time.time - lastUseTime >= cooldown;

    public override float AbilityRange => 30f;

    private float lastUseTime = -999f;
    private ShotgunAttack shotgunAttack;

    private void Start()
    {
        // owner = GetComponentInParent<Character>();
        // animator = owner.GetComponent<Animator>();
        // shotgunAttack = GetComponent<ShotgunAttack>();
    }

    public override void Initialize(Character owner, Animator animator)
    {
        base.Initialize(owner, animator);

        shotgunAttack = GetComponent<ShotgunAttack>();

        List<UpgradeData> upgrades = GameManager.Instance.PlayerUpgrades
            .Where(s => s.UpgradeType == UpgradeData.UpgradeTypes.ShotgunAbilityCooldown)
            .ToList();

        foreach (UpgradeData upgrade in upgrades)
        {
            cooldown += upgrade.AmountToChange;
        }
    }

    public override void Perform()
    {
        if (!CanAttack()) return;
        lastUseTime = Time.time;
        Debug.Log($"ShotgunExplosiveAbility used at {lastUseTime}");
        animator.SetTrigger("Shot");
    }

    public override void DeployProjectile()
    {
        shotgunAttack.UseAmmo(ammoCost);
        if (actionAudio) AudioManager.Instance.PlaySound(actionAudio);

        GameObject projectileGO = Instantiate(explosiveProjectilePrefab, firePoint.position, Quaternion.LookRotation(firePoint.forward));
        projectileGO.GetComponent<ExplosiveProjectile>().Initialize(
            impactDamage, burnDamage, burnRadius, burnDuration, burnTickRate,
            firePoint.forward, projectileSpeed, owner.gameObject, burningZonePrefab
        );
    }

    protected override bool CanAttack()
    {
        if (shotgunAttack.CurrentAmmo < ammoCost) return false;
        if (Time.time - lastUseTime < cooldown) return false;
        return true;
    }

    protected override void IdentifyEnemyInRange(List<IDamageable> entitiesHit) { }
}