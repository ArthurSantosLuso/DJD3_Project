using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTrigger : MonoBehaviour
{
    [SerializeField, SceneDropdown] private string  sceneToOpen;
    [SerializeField] private ScreenFader screenFader;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerMovement>() != null)
        {
            screenFader.FadeAndLoad(sceneToOpen, 1.5f);

        }
    }
}
