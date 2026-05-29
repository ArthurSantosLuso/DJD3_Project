using UnityEngine;
using System.Collections.Generic;

public class ObstacleDetector : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private int maxHits = 10;
    [SerializeField] private float detectionRadius = 0.5f;

    private RaycastHit[] _hitBuffer;
    private HashSet<IFadeable> _currentObstacles = new();
    private HashSet<IFadeable> _nextObstacles = new();

    private void Awake()
    {
        _hitBuffer = new RaycastHit[maxHits];
    }

    private void LateUpdate()
    {
        Vector3 dir = transform.position - cameraTransform.position;

        int hitCount = Physics.SphereCastNonAlloc(
            cameraTransform.position,
            detectionRadius,
            dir.normalized,
            _hitBuffer,
            dir.magnitude,
            obstructionMask
        );

        _nextObstacles.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            IFadeable fade = GetFadeable(_hitBuffer[i].collider);
            if (fade != null)
                _nextObstacles.Add(fade);
        }

        foreach (IFadeable obstacle in _currentObstacles)
        {
            if (!_nextObstacles.Contains(obstacle))
                obstacle.FadeIn();
        }

        foreach (IFadeable obstacle in _nextObstacles)
        {
            if (!_currentObstacles.Contains(obstacle))
                obstacle.FadeOut();
        }

        (_currentObstacles, _nextObstacles) = (_nextObstacles, _currentObstacles);
    }

    private static IFadeable GetFadeable(Collider col)
    {
        FadeObstacleGroup group = col.GetComponentInParent<FadeObstacleGroup>();
        if (group != null) return group;

        return col.GetComponent<FadeObstacle>();
    }

    public void SetCameraTransform(Transform transform)
    {
        cameraTransform = transform;
    }
}