using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainHallInteractables : Interactable
{
    public enum InteractionType { Config, Save, ReadFiles, Leave}

    [Header("Setup")]
    [SerializeField] private InteractionType type;
    [SerializeField] private GameObject canvasToOpen;
    [SerializeField, SceneDropdown] private string sceneToOpen;

    public override void TryInteract(Character entity = null)
    {
        Interact();
    }

    protected override void Interact(Character entity = null)
    {
        OpenMenu();
    }

    private void OpenMenu()
    {
        if (!canvasToOpen.activeSelf)
        {
            canvasToOpen.SetActive(true);
            GameManager.Instance.StopPlayerActions();
        }
    }

    public void OpenScene()
    {
        GameManager.Instance.ActivatePlayerActions();
        SceneManager.LoadScene(sceneToOpen);
    }

    private void SavaGame()
    {
        // Implement
    }
}
