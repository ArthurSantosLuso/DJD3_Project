using System;
using UnityEngine;

public class TriggerNotifier : MonoBehaviour
{
    public event Action<Collider> TriggerEntered;

    private void OnTriggerEnter(Collider other)
    {
        TriggerEntered?.Invoke(other);
    }

    public void EnableHitbox()
    {
        GetComponent<Collider>().enabled = true;
    }

    public void DisableHitbox()
    {
        GetComponent<Collider>().enabled = false;
    }
}
