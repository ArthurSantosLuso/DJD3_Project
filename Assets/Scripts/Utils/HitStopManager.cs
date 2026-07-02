using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance { get; private set; }

    private float       originalFixedDelta;
    private Coroutine   activeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        originalFixedDelta = Time.fixedDeltaTime;
    }

    public void DoTimeEffect(float duration, float timeScale)
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(Routine(duration, timeScale));
    }

    private IEnumerator Routine(float duration, float timeScale)
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = originalFixedDelta * timeScale;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDelta;
        activeRoutine = null;
    }
}
