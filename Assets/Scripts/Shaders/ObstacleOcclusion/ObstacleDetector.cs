using System.Security;
using UnityEngine;

public class ObstacleDetector : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask obstructionMask;

    private IFadeable _currentObstacle;

    private void LateUpdate()
    {
        Vector3 dir = transform.position - cameraTransform.position;

        if (Physics.Raycast(cameraTransform.position, dir.normalized, out RaycastHit hit, dir.magnitude, obstructionMask))
        {
            IFadeable fade = GetFadeable(hit.collider);

            if (fade != null && fade != _currentObstacle)
            {
                _currentObstacle?.FadeIn();
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

    private static IFadeable GetFadeable(Collider col)
    {
        FadeObstacleGroup group = col.GetComponentInParent<FadeObstacleGroup>();
        if (group != null) return group;

        return col.GetComponent<FadeObstacle>();
    }

}
