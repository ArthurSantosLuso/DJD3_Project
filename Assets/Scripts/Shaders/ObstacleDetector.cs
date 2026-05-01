using UnityEngine;

public class ObstacleDetector : MonoBehaviour
{
    public Transform cameraTransform;
    public LayerMask obstructionMask;

    private FadeObstacle _currentObstacle;

    private void LateUpdate()
    {
        Vector3 dir = transform.position - cameraTransform.position;

        if (Physics.Raycast(cameraTransform.position, dir.normalized, out RaycastHit hit, dir.magnitude, obstructionMask))
        {
            FadeObstacle fade = hit.collider.GetComponent<FadeObstacle>();

            if (fade != null && fade != _currentObstacle)
            {
                if (_currentObstacle != null)
                    _currentObstacle.FadeIn();

                fade.FadeOut();
                _currentObstacle = fade;
            }
        }
        else
        {
            if (_currentObstacle != null)
            {
                _currentObstacle.FadeIn();
                _currentObstacle = null;
            }
        }
    }

}
