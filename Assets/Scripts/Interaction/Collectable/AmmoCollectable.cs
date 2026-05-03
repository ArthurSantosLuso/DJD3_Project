using UnityEngine;

public class AmmoCollectable : MonoBehaviour
{
    [Header("Ammo Settings")]
    [SerializeField] private int ammoAmount = 5;


    private void OnTriggerEnter(Collider other)
    {
        // Verifica se é o Player
        ShotgunAttack shotgun = other.GetComponentInChildren<ShotgunAttack>();

        if (shotgun == null)
            shotgun = other.GetComponent<ShotgunAttack>();

        if (shotgun == null) return;

        // Só dá ammo se precisar
        if (shotgun.CurrentAmmo >= shotgun.MaxAmmo) return;

        shotgun.AddAmmo(ammoAmount);

        Destroy(gameObject);
    }
}