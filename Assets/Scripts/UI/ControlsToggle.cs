using UnityEngine;

public class ControlsToggle : MonoBehaviour
{
    [SerializeField] private GameObject controlsPanel; 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            controlsPanel.SetActive(!controlsPanel.activeSelf);
        }
    }
}