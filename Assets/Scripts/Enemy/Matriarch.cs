using UnityEngine;

public class Matriarch : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool onlyRotateY = true;

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;

        if (onlyRotateY)
            direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}