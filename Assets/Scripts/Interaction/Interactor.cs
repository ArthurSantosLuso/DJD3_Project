using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Interactor : MonoBehaviour
{
    private Character character;

    private void Awake() => character = GetComponent<Character>();

    private void OnTriggerEnter(Collider other)
    {
        // Get the interactable
        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable != null && character.ShouldConsiderInteractable)
        {
            interactable.TryInteract(character);
        }
    }
}
