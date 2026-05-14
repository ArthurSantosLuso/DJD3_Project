using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public bool isAutomatic = false;
    public GameObject interactionHint;

    public abstract void TryInteract(Character entity = null);
    protected abstract void Interact(Character entity = null);

    public virtual void ShowPrompt() => interactionHint.SetActive(true);

    public virtual void HidePrompt() => interactionHint.SetActive(false);
}