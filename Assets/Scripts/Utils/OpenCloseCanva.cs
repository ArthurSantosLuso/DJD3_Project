using UnityEngine;

public class OpenCloseCanva : MonoBehaviour
{
    [SerializeField] private GameObject obj;
    [SerializeField] private bool shouldOpen;

    public void Execute()
    {
        obj.SetActive(shouldOpen);
        if (!shouldOpen) GameManager.Instance.ActivatePlayerActions();
    }
}
