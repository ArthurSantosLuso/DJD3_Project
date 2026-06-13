using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct DialogueConfig
{
    public int teddyBearCount;
    public TextAsset inkJSON;
}

public class DialogueInteractable : Interactable
{
    [SerializeField] private bool isDialogueScalable = false;

    [Header("Dialogue Content")]
    [SerializeField] private List<DialogueConfig> scalableDialogues;
    [SerializeField] private TextAsset uniqueDialogue;
    [SerializeField] private Sprite profileImage;

    protected override void Interact(Character entity = null)
    {
        if (uniqueDialogue == null && scalableDialogues == null)
        {
            Debug.LogWarning("NPC has no Ink JSON assigned!");
            return;
        }

        if (isDialogueScalable)
        {
            int teddyBearCount = GameManager.Instance.TeddyBearCount;
            TextAsset text = scalableDialogues.Where(s=> s.teddyBearCount == teddyBearCount).Select(s=> s.inkJSON).FirstOrDefault();
            DialogueManager.Instance.EnterDialogueMode(text, profileImage);
        }
        else
        {
            // Send notification to the manager to start
            DialogueManager.Instance.EnterDialogueMode(uniqueDialogue, profileImage);
        }
    }

    public override void TryInteract(Character entity = null)
    {
        // Don't start a new dialogue if already talking
        if (DialogueManager.Instance.IsDialoguePlaying) return;

        Interact(entity);
    }
}