using System.Security;
using UnityEngine;

public class MainHallInteractables : Interactable
{
    private enum InteractionType { Config, Save, ReadFiles}

    [SerializeField] private InteractionType type;

    public override void TryInteract(Character entity = null)
    {
        Interact();
    }

    protected override void Interact(Character entity = null)
    {
        switch (type)
        {
            case InteractionType.Config:
                ConfigurationsMenu();
                break;

            case InteractionType.Save:
                SaveGame();
                break;

            case InteractionType.ReadFiles:
                ReadFilesMenu();
                break;
        }
    }

    private void ConfigurationsMenu()
    {
        Debug.Log("Config");
    }

    private void SaveGame()
    {
        Debug.Log("Game Saved");
    }

    private void ReadFilesMenu()
    {
        Debug.Log("Files menu opened");
    }
}
