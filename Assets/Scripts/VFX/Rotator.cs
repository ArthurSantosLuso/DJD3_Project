using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotation;
    [SerializeField] private float speed;

    private void Update()
    {
        transform.Rotate(rotation * speed * Time.deltaTime);
    }
}
