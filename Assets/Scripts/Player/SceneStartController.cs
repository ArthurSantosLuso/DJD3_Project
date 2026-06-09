using UnityEngine;
using static UnityEngine.LowLevelPhysics2D.PhysicsLayers;

public class SceneStartController : MonoBehaviour
{
    [SerializeField] private bool shouldStartSitting;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (!shouldStartSitting) GoToDefaultAnimations();
    }

    public void GoToDefaultAnimations()
    {
        int layerIndex = animator.GetLayerIndex("Sit");
        animator.SetLayerWeight(layerIndex, 0f);
        if (shouldStartSitting) GameManager.Instance.ActivatePlayerActions();
        enabled = false;
    }
}
