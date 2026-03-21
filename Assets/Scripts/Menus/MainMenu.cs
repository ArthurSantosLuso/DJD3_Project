using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject quitConfirmation;
    [SerializeField] private GameObject configurations;

    private GameObject currentWindow;

    public void OpenScene(int sceneIdx) => SceneManager.LoadScene(sceneIdx);

    public void QuitGame() => Application.Quit();

    public void SetCurrentWindow(GameObject window) => currentWindow = window;

    public void OpenCloseWindow(bool shouldOpen) => currentWindow.SetActive(shouldOpen);
}
