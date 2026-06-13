using UnityEngine;

public class IgnoreParentRotation : MonoBehaviour
{
    private Quaternion initialWorldRotation;

    private void Start()
    {
        initialWorldRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        transform.rotation = initialWorldRotation;
    }
}
