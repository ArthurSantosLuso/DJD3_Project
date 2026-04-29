using System.Security;
using UnityEngine;

public class Worm : MonoBehaviour
{
    [SerializeField] private float damageValue;
    private bool hasDamaged = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasDamaged) return;

        IDamageable hit = other.GetComponent<IDamageable>();
        if (hit != null)
        {
            hit.Damage(damageValue);
            hasDamaged = true;
        }
    }
}
