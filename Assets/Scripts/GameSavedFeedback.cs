using System.Collections;
using TMPro;
using UnityEngine;

public class GameSavedFeedback : MonoBehaviour
{
    [SerializeField] private TextMeshPro feedbackText;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private AudioClip savedSound;
    private Animator animator;

    public void ShowFeedback()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(ShowRoutine());
    }
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private IEnumerator ShowRoutine()
    {
        animator.Play("GameSavedAppear");
        if (savedSound && AudioManager.Instance != null)
            AudioManager.Instance.PlaySound(savedSound);
        yield return new WaitForSeconds(displayDuration);
        gameObject.SetActive(false);
    }
}