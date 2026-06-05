using UnityEngine;

public class PausableMonoBehaviour : MonoBehaviour
{
    public virtual void Pause()
    {
        this.enabled = false;
    }

    public virtual void UnPause()
    {
        this.enabled = true;
    }
}
