using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTrigger : MonoBehaviour
{
    [SerializeField, SceneDropdown] private string  sceneToOpen;
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private GameObject confirmationPanel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerMovement>() != null)
        {
            if (GameManager.Instance.HasUpgradeAvaliable)
            {
                confirmationPanel.SetActive(true);
                GameManager.Instance.StopPlayerActions();
                return;
            }
            
            screenFader.FadeAndLoad(sceneToOpen, 1.5f);

        }
    }

    public void OpenLevel()
    {
        confirmationPanel.SetActive(false);
        GameManager.Instance.ActivatePlayerActions();
        screenFader.FadeAndLoad(sceneToOpen, 1.5f);
    }
}
