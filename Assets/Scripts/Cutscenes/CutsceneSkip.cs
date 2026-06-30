using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CutsceneSkip : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private int nextSceneIndex;
    [SerializeField] private float holdTimeToSkip = 1.5f;

    [SerializeField] private GameObject skipUI;
    [SerializeField] private Image fillCircle;
    [SerializeField] private ScreenFader screenFader;

    private float holdTimer;

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            holdTimer += Time.deltaTime;
            skipUI.SetActive(true);
            fillCircle.fillAmount = holdTimer / holdTimeToSkip;

            if (holdTimer >= holdTimeToSkip)
            {
                director.Stop();
                screenFader.FadeAndLoad(SceneUtility.GetScenePathByBuildIndex(nextSceneIndex), 1f);
            }
        }
        else
        {
            holdTimer = 0f;
            skipUI.SetActive(false);
            fillCircle.fillAmount = 0f;
        }
    }
}