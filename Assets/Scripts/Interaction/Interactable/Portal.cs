using System;
using Unity.Cinemachine;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Action<bool> OnPlayerTeleport;

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
        PortalFader fader = FindFirstObjectByType<PortalFader>(FindObjectsInactive.Include);
        fader.FadeInOut(() =>
        {
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

            CinemachineCamera cam = FindFirstObjectByType<CinemachineCamera>();
            if (cam != null)
                cam.OnTargetObjectWarped(player.transform, destination.position - player.transform.position);

            OnPlayerTeleport?.Invoke(true);
            StartCoroutine(CooldownRoutine());
        });
    }

    private System.Collections.IEnumerator CooldownRoutine()
    {
        onCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        Destroy(gameObject);
    }
}