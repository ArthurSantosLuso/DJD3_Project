using UnityEngine;
using UnityEngine.SceneManagement;

public class MainHallInteractables : Interactable
{
    public enum InteractionType { Config, Save, Upgrade, Leave}

    [Header("Setup")]
    [SerializeField] private InteractionType type;
    [SerializeField] private GameObject canvasToOpen;
    [SerializeField, SceneDropdown] private string sceneToOpen;
    [SerializeField] private GameObject upgradePropsContainer;
 
    private void Start()
    {
        if (type == InteractionType.Upgrade)
        {
            UpdateUpgradeProps();
        }
    }

    public override void TryInteract(Character entity = null)
    {
        Interact();
    }

    protected override void Interact(Character entity = null)
    {
        if (type == InteractionType.Save)
        {
            SavaGame();
            return;
        }

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

    public void SavaGame()
    {
        GameManager.Instance.SaveGame();
    }

    private void UpdateUpgradeProps()
    {
        if (!GameManager.Instance.HasUpgradeAvaliable)
            upgradePropsContainer.SetActive(false);
        else upgradePropsContainer.SetActive(true);
    }
}
