using UnityEngine;

public class UncleBenController : MonoBehaviour
{
    [SerializeField] private Transform defaultPosition;
    [SerializeField] private Transform initialPosition;

    private void Start()
    {
        if (GameManager.Instance.TeddyBearCount == 0)
        {
            transform.position = initialPosition.position;
        }
        else
        {
            transform.position = defaultPosition.position;
        }
    }
}
