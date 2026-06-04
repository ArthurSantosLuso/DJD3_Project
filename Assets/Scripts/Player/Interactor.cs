using UnityEngine;

public class Interactor : PausableMonoBehaviour
{
    private Character character;
    private Interactable currentInteractable;

    private void Awake() => character = GetComponent<Character>();

    private void OnTriggerEnter(Collider other)
    {
        Interactable interactable = other.gameObject.GetComponentInParent<Interactable>();

        if (interactable != null && character.ShouldConsiderInteractable)
        {
            Debug.Log("Entrei num collider trigger Interactable!");
            if (interactable.IsAutomatic)
            {
                interactable.TryInteract(character);
            }
            else
            {
                Outline objOutline = GetComponentInParent<Outline>()
                                ?? GetComponent<Outline>()
                                ?? GetComponentInChildren<Outline>();

                objOutline?.ToggleOutline(true);
                currentInteractable = interactable;
                currentInteractable.ShowPrompt();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Interactable interactable = other.GetComponentInParent<Interactable>();
        if (interactable != null && interactable == currentInteractable)
        {
            currentInteractable.HidePrompt();
            currentInteractable = null;

            Outline objOutline = GetComponentInParent<Outline>()
                ?? GetComponent<Outline>()
                ?? GetComponentInChildren<Outline>();

            objOutline?.ToggleOutline(false);
        }
    }

    public void ExecuteInteraction()
    {
        if (currentInteractable != null)
        {
            currentInteractable.TryInteract(character);
        }
    }
}