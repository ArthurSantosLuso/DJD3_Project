using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyLightAttack : Ability
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

    public override void DeployProjectile()
    {
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;

        GameObject player = GameManager.Instance.Player;

        for (int i = 0; i < pelletsPerShot; i++)
        {
            Vector3 dirToPlayer = (player.transform.position - spawnPosition).normalized;

            Vector3 spreadDir = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0f) * dirToPlayer;

            GameObject p = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
            p.GetComponent<Bullet>().Initialize(spreadDir, bulletSpeed, damageAmount, gameObject);
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