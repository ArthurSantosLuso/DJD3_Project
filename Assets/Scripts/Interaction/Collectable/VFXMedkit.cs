using UnityEngine;

public class VFXMedkit : MonoBehaviour
{
    private float timeToSpin;
    private float timer;
    private Animator animator;

    private void Start()
    {
        timeToSpin = Random.Range(2f, 5f);
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= timeToSpin)
        {
            timer = 0;
            animator.SetTrigger("Spin");
        }
    }
}
