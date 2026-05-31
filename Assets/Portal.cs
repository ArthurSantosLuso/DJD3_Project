using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private float cooldownDuration = 2f;

    private Transform destination;
    private bool onCooldown = false;

    /// <summary>
    /// Sets where the player will land after passing through this portal.
    /// Called by RoomManager after spawning.
    /// </summary>
    public void SetDestination(Transform destination)
    {
        this.destination = destination;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (onCooldown) return;
        if (!other.CompareTag("Player")) return;
        if (destination == null)
        {
            Debug.LogWarning($"Portal on {gameObject.name} has no destination set.");
            return;
        }

        TeleportPlayer(other.gameObject);
    }

    private void TeleportPlayer(GameObject player)
    {
        // CharacterController overrides transform position every frame, so it must be
        // disabled before moving and re-enabled after, otherwise the teleport is ignored
        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc != null)
        {
            cc.enabled = false;
            player.transform.SetPositionAndRotation(destination.position, destination.rotation);
            cc.enabled = true;
        }
        else
        {
            player.transform.SetPositionAndRotation(destination.position, destination.rotation);
        }

        StartCoroutine(CooldownRoutine());
    }

    private System.Collections.IEnumerator CooldownRoutine()
    {
        onCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        onCooldown = false;
    }
}