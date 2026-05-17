using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Interactor : MonoBehaviour
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
            if (interactable.isAutomatic)
            {
                interactable.TryInteract(character);
            }
            else
            {
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