using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutsceneSceneSwitcher : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private string nextSceneName;

    private void OnEnable()
    {
        director.stopped += OnTimelineFinished;
    }

    private void OnDisable()
    {
        director.stopped -= OnTimelineFinished;
    }

    private void OnTimelineFinished(PlayableDirector pd)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}