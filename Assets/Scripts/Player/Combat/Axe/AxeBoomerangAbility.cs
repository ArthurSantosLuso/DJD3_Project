using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AxeBoomerangAbility : Ability
{
    [Header("Boomerang Settings")]
    [SerializeField] private GameObject axePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float damageAmount = 30f;
    [SerializeField] private float enemyDetectionRadius = 8f;
    [SerializeField] private float cooldown = 4f;

    [Header("Axe Visuals")]
    [SerializeField] private MeshRenderer axeMeshRenderer;

    [Header("Enemy Detection")]
    [SerializeField] private LayerMask enemyLayer;

    public override float AbilityRange => enemyDetectionRadius;

    private float lastUseTime = -999f;
    private bool axeOut = false;
    private PlayerLightMeleeAttack meleeAttack;
    private PlayerStamina playerStamina;

    public override void Initialize(Character owner, Animator animator)
    {
        base.Initialize(owner, animator);
        meleeAttack = GetComponent<PlayerLightMeleeAttack>();

        foreach (ValueBase valBase in owner.ValueBases)
        {
            if (valBase is PlayerStamina stamina)
            {
                playerStamina = stamina;
                break;
            }
        }

        List<UpgradeData> upgrades = GameManager.Instance.PlayerUpgrades
            .Where(s => s.UpgradeType == UpgradeData.UpgradeTypes.AxeCooldown)
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
        axeOut = true;
        owner.ChangeState(Character.State.UsingAbility);
        Debug.Log($"State changed to: {owner.CharacterState}");
        playerStamina.UseStamina(staminaCost);
        meleeAttack.enabled = false;
        axeMeshRenderer.enabled = false;

        Transform target = FindTarget();

        GameObject axe = Instantiate(axePrefab, firePoint.position, axePrefab.transform.rotation);
        axe.GetComponent<BoomerangAxe>().Initialize(owner.gameObject, owner.transform, target, firePoint.forward, projectileSpeed, damageAmount, OnAxeReturned);
    }

    private Transform FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(owner.transform.position, enemyDetectionRadius, enemyLayer);
        Transform bestTarget = null;
        float bestScore = -1f;

        foreach (Collider hit in hits)
        {
            if (hit.GetComponent<IDamageable>() == null) continue;

            float distance = Vector3.Distance(owner.transform.position, hit.transform.position);
            Vector3 toTarget = (hit.transform.position - owner.transform.position).normalized;
            float angle = Vector3.Angle(owner.transform.forward, toTarget);
            float score = (1f - angle / 180f) * 0.7f + (1f - distance / enemyDetectionRadius) * 0.3f;

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = hit.transform;
            }
        }

        return bestTarget;
    }

    private void OnAxeReturned()
    {
        axeOut = false;
        meleeAttack.enabled = true;
        axeMeshRenderer.enabled = true;
        owner.ChangeState(Character.State.Normal);
    }

    protected override bool CanAttack()
    {
        if (axeOut) return false;
        if (Time.time - lastUseTime < cooldown) return false;
        return playerStamina.HasStamina(staminaCost);
    }

    protected override void IdentifyEnemyInRange(List<IDamageable> entitiesHit) { }
}