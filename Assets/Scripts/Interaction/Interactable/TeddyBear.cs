using UnityEngine;
using UnityEngine.SceneManagement;

public class TeddyBear : Interactable
{
    [SerializeField, SceneDropdown] private string sceneToOpen;

    private bool canInteract = true;

    public override void TryInteract(Character entity = null)
    {
        if (canInteract)
        {
            Interact();
        }
    }

    protected override void Interact(Character entity = null)
    {
        LevelManager.Instance.FinishLevel();
    }
}
