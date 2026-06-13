using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject quitConfirmation;
    [SerializeField] private GameObject configurations;
    [SerializeField, SceneDropdown] private string sceneToOpen;

    private GameObject currentWindow;

    public void OpenScene() => SceneManager.LoadScene(sceneToOpen);

    public void QuitGame() => Application.Quit();

    public void SetCurrentWindow(GameObject window) => currentWindow = window;

    public void OpenCloseWindow(bool shouldOpen) => currentWindow.SetActive(shouldOpen);
}
