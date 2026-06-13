using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : ValueBase, IDamageable
{
    public System.Action<EnemyHealth> OnDeath;

    private HitFlash hitFlash;
    private EnemyBaseAI enemyAI;
    private bool isArmUnplugged = false;
    private bool isDead = false;

    // Tracks the original materials per mesh object so we can restore them on reuse
    private List<Material> originalMaterials = new List<Material>();
    private Coroutine disintegrationCoroutine;

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

        enemyAI = GetComponent<EnemyBaseAI>();
        renderImage = GameObject.FindWithTag("Render Texture").GetComponent<RawImage>();

        GameObject canvasObj = GameObject.FindWithTag(healthBarCanvasTag);
        if (canvasObj != null)
            healthBarCanvas = canvasObj.GetComponent<Canvas>();
        else
            Debug.LogWarning($"[EnemyHealth] No GameObject found with tag '{healthBarCanvasTag}'");

        CacheOriginalMaterials();
    }

    /// <summary>
    /// Stores each mesh object's original material so PrepareForReuse can restore them.
    /// </summary>
    private void CacheOriginalMaterials()
    {
        originalMaterials.Clear();

        if (enemyMeshObjects == null) return;

        foreach (GameObject meshObj in enemyMeshObjects)
        {
            if (meshObj == null)
            {
                originalMaterials.Add(null);
                continue;
            }

            Renderer rend = meshObj.GetComponent<Renderer>();
            originalMaterials.Add(rend != null ? rend.sharedMaterial : null);
        }
    }

    public void Damage(float damageValue)
    {
        if (isDead) return;

        base.ReduceValue(damageValue);

        hitFlash?.Flash();
        enemyAI?.TriggerStagger();

        HandleHealthBar();
        VerifyLife();
    }

    private void HandleHealthBar()
    {
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
        isDead = true;

        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance.gameObject);
            healthBarInstance = null;
        }

        if (dropRate >= Random.value)
        {
            float drop = Random.value;
            Instantiate(drop > 0.5f ? drops[1] : drops[0], transform.position, Quaternion.identity);
        }

        // Stop any previously running coroutine before starting a new one
        if (disintegrationCoroutine != null)
            StopCoroutine(disintegrationCoroutine);

        disintegrationCoroutine = StartCoroutine(StartDisintegration());
    }

    private IEnumerator StartDisintegration()
    {
        if (enemyAI != null) enemyAI.enabled = false;

        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null) mainCollider.enabled = false;

        Animator enemyAnimator = GetComponentInChildren<Animator>();
        if (enemyAnimator != null) enemyAnimator.enabled = false;

        if (disintegrationMaterial != null && enemyMeshObjects != null)
        {
            List<Material> materialsToDissolve = new List<Material>();

            foreach (GameObject meshObj in enemyMeshObjects)
            {
                if (meshObj == null) continue;

                Renderer rend = meshObj.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material = disintegrationMaterial;

                    if (rend.material.HasProperty("_Dissolve"))
                        materialsToDissolve.Add(rend.material);
                }
            }

            float dissolveDuration = 2.0f;
            float elapsedTime = 0f;

            while (elapsedTime < dissolveDuration)
            {
                elapsedTime += Time.deltaTime;
                float newCutoff = Mathf.Lerp(0f, 0.8f, elapsedTime / dissolveDuration);

                foreach (Material mat in materialsToDissolve)
                    mat.SetFloat("_Dissolve", newCutoff);

                yield return null;
            }
        }

        disintegrationCoroutine = null;

        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }

    /// <summary>
    /// Resets this enemy to a clean alive state so it can be reused from the pool.
    /// Called by AgentPoolManager before handing the agent out again.
    /// </summary>
    public void PrepareForReuse()
    {
        // Stop dissolve if it's somehow still running
        if (disintegrationCoroutine != null)
        {
            StopCoroutine(disintegrationCoroutine);
            disintegrationCoroutine = null;
        }

        isDead = false;
        isArmUnplugged = false;

        // Restore original materials
        if (enemyMeshObjects != null)
        {
            for (int i = 0; i < enemyMeshObjects.Count; i++)
            {
                if (enemyMeshObjects[i] == null) continue;

                Renderer rend = enemyMeshObjects[i].GetComponent<Renderer>();
                if (rend != null && i < originalMaterials.Count && originalMaterials[i] != null)
                    rend.material = originalMaterials[i];
            }
        }

        // Re-enable components disabled during death
        if (enemyAI != null) enemyAI.enabled = true;

        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null) mainCollider.enabled = true;

        Animator enemyAnimator = GetComponentInChildren<Animator>();
        if (enemyAnimator != null)
        {
            enemyAnimator.enabled = true;
            enemyAnimator.Rebind();  // resets animation state to defaults
            enemyAnimator.Update(0f);
        }

        // Reset arm state
        if (shouldArmUnplug && originalArm != null && armUnplugged != null)
        {
            armUnplugged.SetActive(false);
            originalArm.SetActive(true);
        }

        // Destroy any leftover health bar
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance.gameObject);
            healthBarInstance = null;
        }

        // TODO: reset currentValue to maxValue once ValueBase is available
        // e.g. ResetValue(); or SetValue(maxValue);
    }

    public bool CanDamage() => currentValue > 0;
    public bool HasBlood() => true;

    public void DamageNoStagger(float damageValue)
    {
        if (isDead) return;

        base.ReduceValue(damageValue);
        hitFlash?.Flash();
        HandleHealthBar();
        VerifyLife();
    }
}