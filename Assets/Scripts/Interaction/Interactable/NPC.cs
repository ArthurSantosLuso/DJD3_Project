using UnityEngine;

public class NPC : Interactable
{
    [Header("Dialogue Content")]
    [SerializeField] private TextAsset inkJSON;
    [SerializeField] private Sprite profileImage;

    protected override void Interact(Character entity = null)
    {
        if (inkJSON == null)
        {
            Debug.LogWarning("NPC has no Ink JSON assigned!");
            return;
        }

        // Send notification to the manager to start
        DialogueManager.Instance.EnterDialogueMode(inkJSON, profileImage);
    }

    public override void TryInteract(Character entity = null)
    {
        // Don't start a new dialogue if we are already talking
        if (DialogueManager.Instance.IsDialoguePlaying) return;

        Interact(entity);
    }
}