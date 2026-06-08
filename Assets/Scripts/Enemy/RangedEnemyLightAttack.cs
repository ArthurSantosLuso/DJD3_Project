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

    public override float AbilityRange => throw new System.NotImplementedException();

    public override void Perform()
    {
        owner.PlayAnimation("Attack");
    }

    public override void DeployProjectile()
    {
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;

        for (int i = 0; i < pelletsPerShot; i++)
        {
            float randomX = Random.Range(-spreadAngle, spreadAngle);
            float randomY = Random.Range(-spreadAngle, spreadAngle);
            Vector3 spreadDirection = Quaternion.Euler(randomX, randomY, 0f) * firePoint.forward;

            GameObject p = Instantiate(bulletPrefab, spawnPosition, Quaternion.LookRotation(spreadDirection));
            p.GetComponent<Bullet>().Initialize(spreadDirection, bulletSpeed, damageAmount, gameObject);
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