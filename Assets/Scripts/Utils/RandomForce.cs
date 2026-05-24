using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RandomForce : MonoBehaviour
{
    [Header("Strenght")]
    [SerializeField] private float minForce = 5f;
    [SerializeField] private float maxForce = 15f;
    [SerializeField] private float randomTorque = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Vector3 randomDirection = Random.onUnitSphere;

        float forceAmount = Random.Range(minForce, maxForce);

        rb.AddForce(randomDirection * forceAmount, ForceMode.Impulse);

        Vector3 randomRotation = Random.insideUnitSphere * randomTorque;

        rb.AddTorque(randomRotation, ForceMode.Impulse);
    }
}