using System.Collections;
using UnityEngine;
public class FadeObstacle : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float fadeOutAlpha = 0.2f;
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private Material _material;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        _material = GetComponent<Renderer>().material;
    }

    public void FadeOut()
    {
        StartFade(fadeOutAlpha);
    }

    public void FadeIn()
    {
        StartFade(1f);
    }

    private void StartFade(float targetAlpha)
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeCoroutine(targetAlpha));
    }

    private IEnumerator FadeCoroutine(float targetAlpha)
    {
        Color c = _material.GetColor(BaseColorID);
        float startAlpha = c.a;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            float a = Mathf.Lerp(startAlpha, targetAlpha, t);
            c.a = a;
            _material.SetColor(BaseColorID, c);
            yield return null;
        }

        c.a = targetAlpha;
        _material.SetColor(BaseColorID, c);
        _fadeRoutine = null;
    }
}