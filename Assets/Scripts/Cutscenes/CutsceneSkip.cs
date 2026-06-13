using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CutsceneSkip : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private string nextSceneName;
    [SerializeField] private float holdTimeToSkip = 1.5f;

    [SerializeField] private GameObject skipUI;
    [SerializeField] private Image fillCircle;

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
                SceneManager.LoadScene(nextSceneName);
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