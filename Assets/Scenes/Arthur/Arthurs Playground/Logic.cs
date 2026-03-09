using UnityEngine;
using UnityEngine.UI;

public class Logic : MonoBehaviour
{
    [SerializeField]
    private GameObject axeHand;

    [SerializeField]
    private GameObject axeBack;

    private Animator animator;
    private bool isEquipped = false;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.J))
        {
            isEquipped = !isEquipped;
            animator.SetBool("isEquipped", isEquipped);
            animator.SetTrigger("axeOnOff");
        }

        if (Input.GetKeyUp(KeyCode.K))
        {
            animator.SetTrigger("attack");
        }
    }

    public void EquipAxe()
    {
        axeHand.SetActive(true);
        axeBack.SetActive(false);
    }

    public void UnequipAxe()
    {
        axeHand.SetActive(false);
        axeBack.SetActive(true);
    }
}
