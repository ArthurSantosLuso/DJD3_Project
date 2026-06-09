using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ShotgunAttack : Ability, IAmmoProvider
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float damageAmount = 25f;

    [Header("Shotgun Settings")]
    [SerializeField] private int pelletsPerShot = 6;
    [SerializeField] private float spreadAngle = 15f;
    [SerializeField] private float fireRate = 0.8f;

    [Header("Ammo Settings")]
    [SerializeField] private int maxAmmo = 20;
    private int currentAmmo;

    [Header("Blood FX")]
    [SerializeField] private GameObject bloodEffectPrefab;

    public override float AbilityRange => 20f;

    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => maxAmmo;

    private float lastFireTime = -999f;
    private PlayerStamina playerStamina;

    public event System.Action<int, int> OnAmmoChanged;

    public override void Initialize(Character owner, Animator animator)
    {
        base.Initialize(owner, animator);

        foreach (ValueBase valBase in owner.ValueBases)
        {
            if (valBase is PlayerStamina stamina)
            {
                playerStamina = stamina;
                break;
            }
        }

        List<UpgradeData> upgrades = GameManager.Instance.PlayerUpgrades
            .Where(s => s.UpgradeType == UpgradeData.UpgradeTypes.ShotgunDamage)
            .ToList();

        foreach (UpgradeData upgrade in upgrades)
        {
            damageAmount += upgrade.AmountToChange;
        }

        currentAmmo = maxAmmo;
    }

    public override void Perform()
    {
        if (!CanAttack()) return;

        lastFireTime = Time.time;
        animator.SetTrigger("Shot");
    }

    public override void DeployProjectile()
    {
        currentAmmo--;
        if (actionAudio) AudioManager.Instance.PlaySound(actionAudio);

        for (int i = 0; i < pelletsPerShot; i++)
        {
            float randomX = Random.Range(-spreadAngle, spreadAngle);
            float randomY = Random.Range(-spreadAngle, spreadAngle);
            Vector3 spreadDirection = Quaternion.Euler(randomX, randomY, 0f) * firePoint.forward;

            GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(spreadDirection));
            bulletGO.GetComponent<Bullet>().Initialize(spreadDirection, bulletSpeed, damageAmount, owner.gameObject, bloodEffectPrefab);
        }
        ChangeAmmoUI();
    }

    protected override bool CanAttack()
    {
        if (currentAmmo <= 0) return false;
        if (Time.time - lastFireTime < fireRate) return false;
        return playerStamina.HasStamina(staminaCost);
    }

    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        ChangeAmmoUI();
    }

    private void ChangeAmmoUI()
    {
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
    }
    public void UseAmmo(int amount)
    {
        currentAmmo = Mathf.Max(currentAmmo - amount, 0);
        ChangeAmmoUI();
    }

    protected override void IdentifyEnemyInRange(List<IDamageable> entitiesHit) { }
}