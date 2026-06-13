using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : ValueBase, IDamageable
{
    public System.Action<EnemyHealth> OnDeath;

    private HitFlash hitFlash;
    private EnemyBaseAI enemyAI;
    private bool isArmUnplugged = false;

    [SerializeField] private bool shouldArmUnplug = false;
    [SerializeField] private GameObject armUnplugged;
    [SerializeField] private GameObject originalArm;
    [SerializeField] private List<GameObject> drops;
    [SerializeField]
    [Range(0.0f, 1f)] private float dropRate;

    [Header("Health Bar")]
    [Tooltip("Prefab that contains EnemyHealthBar component")]
    [SerializeField] private GameObject healthBarPrefab;

    [Header("Death Effects")]
    [SerializeField] private Material disintegrationMaterial;

    [Tooltip("Child with mesh renderer")]
    [SerializeField] private List<GameObject> enemyMeshObjects;

    [Tooltip("The RawImage that displays the render texture.")]
    [SerializeField] private UnityEngine.UI.RawImage renderImage;

    [Tooltip("Tag on the overlay canvas that holds all enemy health bars.")]
    [SerializeField] private string healthBarCanvasTag = "EnemyHealthBarCanvas";

    private EnemyHealthBar healthBarInstance;
    private Canvas healthBarCanvas;

    private void Start()
    {
        if (shouldArmUnplug)
        {
            armUnplugged?.SetActive(false);
            originalArm.SetActive(true);
        }

        hitFlash = GetComponent<HitFlash>();
        enemyAI = GetComponent<EnemyBaseAI>();

        renderImage = GameObject.FindWithTag("Render Texture").GetComponent<RawImage>();

        // Cache the overlay canvas
        GameObject canvasObj = GameObject.FindWithTag(healthBarCanvasTag);
        if (canvasObj != null)
            healthBarCanvas = canvasObj.GetComponent<Canvas>();
        else
            Debug.LogWarning($"[EnemyHealth] No GameObject found with tag '{healthBarCanvasTag}'");
    }

    public void Damage(float damageValue)
    {
        base.ReduceValue(damageValue);

        hitFlash?.Flash();
        enemyAI.TriggerStagger();

        // Show the health bar after the first hit
        // Update it on every subsequent hit
        HandleHealthBar();

        VerifyLife();
    }


    /// <summary>
    /// Instantiates the health bar if it doesn't exist yet,
    /// then refreshes its fill value.
    /// </summary>
    private void HandleHealthBar()
    {
        // Only show the bar when health is below maximum
        if (currentValue >= maxValue) return;

        if (healthBarInstance == null)
            SpawnHealthBar();

        healthBarInstance?.UpdateHealthSegments(currentValue, maxValue);
    }

    private void SpawnHealthBar()
    {
        if (healthBarPrefab == null || healthBarCanvas == null) return;

        GameObject barObj = Instantiate(healthBarPrefab, healthBarCanvas.transform);
        healthBarInstance = barObj.GetComponent<EnemyHealthBar>();

        if (healthBarInstance == null)
        {
            Debug.LogError("[EnemyHealth] healthBarPrefab does not have an EnemyHealthBar component!");
            return;
        }

        healthBarInstance.Initialize(transform, healthBarCanvas, renderImage);
        healthBarInstance.SetVisible(true);
    }

    private void VerifyLife()
    {
        if (currentValue <= 0)
        {
            Kill();
            return;
        }

        if (shouldArmUnplug && currentValue < 50 && !isArmUnplugged)
        {
            armUnplugged.transform.parent = null;
            isArmUnplugged = true;
            armUnplugged.SetActive(true);
            originalArm.SetActive(false);
        }
    }

    private void Kill()
    {
        // Destroy the health bar before destroying the enemy
        if (healthBarInstance != null)
            Destroy(healthBarInstance.gameObject);

        if (dropRate >= Random.value)
        {
            float drop = Random.value;

            if (drop > 0.5f)
                Instantiate(drops[1], transform.position, Quaternion.identity);
            else
                Instantiate(drops[0], transform.position, Quaternion.identity);
        }

        OnDeath?.Invoke(this);
       
        StartCoroutine(StartDisintegration());
    }

    private System.Collections.IEnumerator StartDisintegration()
    {

        if (enemyAI != null) enemyAI.enabled = false;

        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null) mainCollider.enabled = false;

        Animator enemyAnimator = GetComponentInChildren<Animator>();
        if (enemyAnimator != null) enemyAnimator.enabled = false;

        List<Material> materialsToDissolve = new List<Material>();

        if (disintegrationMaterial != null && enemyMeshObjects != null)
        {
            foreach (GameObject meshObj in enemyMeshObjects)
            {
                if (meshObj == null) continue;

                Renderer rend = meshObj.GetComponent<Renderer>();

                if (rend != null)
                {
                    // material change
                    rend.material = disintegrationMaterial;


                    if (rend.material.HasProperty("_Dissolve"))
                    {
                        materialsToDissolve.Add(rend.material);
                    }
                }
            }

            float dissolveDuration = 2.0f;
            float elapsedTime = 0f;

            while (elapsedTime < dissolveDuration)
            {
                elapsedTime += Time.deltaTime;
                float newCutoff = Mathf.Lerp(0f, 0.8f, elapsedTime / dissolveDuration);

                foreach (Material mat in materialsToDissolve)
                {
                    mat.SetFloat("_Dissolve", newCutoff);
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }

    public bool CanDamage()
    {
        return currentValue > 0;
    }

    public void DamageNoStagger(float damageValue)
    {
        base.ReduceValue(damageValue);
        hitFlash?.Flash();
        HandleHealthBar();
        VerifyLife();
    }
}