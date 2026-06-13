using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenSceneScript : MonoBehaviour
{
    [SerializeField, SceneDropdown] private string sceneToOpen;
    public void OpenScene()
    {
        GameManager.Instance.ActivatePlayerActions();
        SceneManager.LoadScene(sceneToOpen);
    }
}
