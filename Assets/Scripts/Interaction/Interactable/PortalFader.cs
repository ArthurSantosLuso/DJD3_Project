using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PortalFader : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float midpointDelay = 0.5f;

    public void FadeInOut(Action onMidpoint)
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeRoutine(onMidpoint));
    }

    private IEnumerator FadeRoutine(Action onMidpoint)
    {
        fadeImage.gameObject.SetActive(true);

        float t = 0;
        Color c = fadeImage.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = t / duration;
            fadeImage.color = c;
            yield return null;
        }

        onMidpoint?.Invoke();
        yield return new WaitForSeconds(midpointDelay);

        t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = 1f - (t / duration);
            fadeImage.color = c;
            yield return null;
        }

        fadeImage.gameObject.SetActive(false);
    }
}