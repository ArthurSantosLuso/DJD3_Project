using UnityEngine;

public class EnterExitPlayAudio : MonoBehaviour
{
    [SerializeField] private AudioClip enterAudio;
    [SerializeField] private AudioClip exitAudio;

    private void OnTriggerEnter(Collider other)
    {
        if (enterAudio != null)
        {
            AudioManager.Instance.PlaySound(enterAudio, Random.Range(0.9f, 1.1f));
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (exitAudio != null)
        {
            AudioManager.Instance.PlaySound(exitAudio, Random.Range(0.9f, 1.1f));
        }
    }
}
