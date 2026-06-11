using UnityEngine;

public class AmmoCollectable : Interactable
{
    [SerializeField]
    private AmmoBoxData data;

    [SerializeField] private AudioClip collectSound;

    public override void TryInteract(Character entity = null)
    {
        if (entity == null)
        {
            Debug.LogError("No character sent to Health Kit.");
            return;
        }
        else
        {
            Interact(entity);
        }
    }

    protected override void Interact(Character entity = null)
    {
        // Verify if it is the player
        ShotgunAttack shotgun = entity.GetComponentInChildren<ShotgunAttack>();

        if (shotgun == null)
            shotgun = entity.GetComponent<ShotgunAttack>();

        if (shotgun == null) return;

        // Just increase player ammo if needed
        if (shotgun.CurrentAmmo >= shotgun.MaxAmmo) return;

        shotgun.AddAmmo(data.ammoAmout);

        if (collectSound && AudioManager.Instance != null)
            AudioManager.Instance.PlaySound(collectSound);

        Destroy(gameObject);
    }
}