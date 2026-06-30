using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneSceneSwitcher : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private int nextSceneIndex;
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private float fadeBeforeEnd = 1f;

    private void OnEnable()
    {
        director.stopped += OnTimelineFinished;
    }

    private void OnDisable()
    {
        director.stopped -= OnTimelineFinished;
    }
    private void Start()
    {
        StartCoroutine(FadeBeforeEnd());
    }
    private IEnumerator FadeBeforeEnd()
    {
        float fadeTime = (float)director.duration - fadeBeforeEnd;
        yield return new WaitForSeconds(fadeTime);
        screenFader.FadeAndLoad(SceneUtility.GetScenePathByBuildIndex(nextSceneIndex), fadeBeforeEnd);
    }
    private void OnTimelineFinished(PlayableDirector pd) { }
}