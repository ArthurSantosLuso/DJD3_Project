using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public abstract void TryInteract(Character entity = null);

    protected abstract void Interact(Character entity = null);
}
