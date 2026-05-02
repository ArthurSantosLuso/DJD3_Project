using UnityEngine;

public class HitEffect : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            float clipLenght = animator.GetCurrentAnimatorStateInfo(0).length;
            Destroy(gameObject, clipLenght + 0.2f);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
