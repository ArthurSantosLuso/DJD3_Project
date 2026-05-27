using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTrigger : MonoBehaviour
{
    [SerializeField, SceneDropdown] private string sceneToOpen;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerMovement>() != null)
            SceneManager.LoadScene(sceneToOpen);
    }
}
