using UnityEngine;

public class FinalLevelGateController : MonoBehaviour
{
    [SerializeField] private GameObject gate;

    private void OnEnable()
    {
        if (GameManager.Instance.TeddyBearCount > 5)
            gate.SetActive(false);

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
            gate.SetActive(false);
    }
}
