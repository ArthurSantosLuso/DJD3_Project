using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Mono.Cecil.Cil;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private Image fadeImage;

    private void Start()
    {
        fadeImage.color = new(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1.5f);
        StartCoroutine(FadeOut());
    }

    public void FadeAndLoad(string sceneName, float duration)
    {
        fadeImage.gameObject.SetActive(true);
        StartCoroutine(Fader(sceneName, duration));
    }

    private IEnumerator Fader(string sceneName, float duration)
    {
        float t = 0;
        Color c = fadeImage.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = t / duration;
            fadeImage.color = c;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeOut()
    {
        float t = 0;
        Color c = fadeImage.color;

        while (t < 1.5f)
        {
            t += Time.deltaTime;
            c.a = 1f - (t / 1.5f);
            fadeImage.color = c;
            yield return null;
        }

        fadeImage.gameObject.SetActive(false);
    }
}
