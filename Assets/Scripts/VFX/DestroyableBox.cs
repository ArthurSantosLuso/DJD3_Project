using System.Collections.Generic;
using UnityEngine;

public class DestroyableBox : MonoBehaviour, IDamageable
{
    [Header("Box Settings")]
    [Tooltip("Drop your broken box prefab here (the one with RandomForce on its pieces)")]
    [SerializeField] private GameObject fracturedBoxPrefab;

    [Header("Drop Settings")]
    [Tooltip("Chance to drop an item (0 = 0%, 0.5 = 50%, 1 = 100%)")]
    [Range(0f, 1f)]
    [SerializeField] private float dropRate = 0.5f;

    [Tooltip("List of items this box can spawn. Element 0 is common, Element 1 is rare.")]
    [SerializeField] private List<GameObject> drops;

    public bool CanDamage()
    {
        return true;
    }

    public void Damage(float damageValue)
    {
        BreakBox();
    }

    public void DamageNoStagger(float damageValue)
    {
        BreakBox();
    }

    public bool HasBlood()
    {
        return false;
    }

    private void BreakBox()
    {

        if (fracturedBoxPrefab != null)
        {
            Instantiate(fracturedBoxPrefab, transform.position, transform.rotation);
        }


        HandleItemDrops();


        Destroy(gameObject);
    }

    private void HandleItemDrops()
    {

        if (drops == null || drops.Count < 2) return;


        if (dropRate >= Random.value)
        {
            float drop = Random.value;

            if (drop > 0.5f)
            {
                if (drops[1] != null)
                    Instantiate(drops[1], transform.position, Quaternion.identity);
            }
            else
            {
                if (drops[0] != null)
                    Instantiate(drops[0], transform.position, Quaternion.identity);
            }
        }
    }
}