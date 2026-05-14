using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    [SerializeField] private float lifeTime = 1.5f; 
    private void OnEnable()
    {
        Destroy(gameObject, lifeTime);
    }
}
