using UnityEngine;

public class FadeObstacleGroup : MonoBehaviour, IFadeable
{
    private FadeObstacle[] _fadeObstacles;

    private void Awake()
    {
        _fadeObstacles = GetComponentsInChildren<FadeObstacle>(includeInactive: true);

        if (_fadeObstacles.Length == 0)
        {
            Debug.LogWarning($"No FadeObstacle components found in children of {gameObject.name}", this);
        }
    }

    public void FadeIn()
    {
        foreach (FadeObstacle obstacle in _fadeObstacles)
        {
            obstacle.FadeIn(); 
        }
    }

    public void FadeOut()
    {
        foreach (FadeObstacle obstacle in _fadeObstacles)
        {
            obstacle.FadeOut();
        }
    }
}
