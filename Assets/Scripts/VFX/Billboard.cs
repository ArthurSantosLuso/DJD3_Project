using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;           
    }

    private void LateUpdate()
    {
        transform.rotation = mainCam.transform.rotation;
    }
}
