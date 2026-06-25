using System.Collections.Generic;
using TMPro;
using UnityEngine.Playables;
using UnityEngine;

[System.Serializable]
public class SubtitleEntry
{
    public float startTime;
    public float endTime;
    public string text;
}

public class SubtitleManager : MonoBehaviour
{
    public PlayableDirector director;
    public TextMeshProUGUI subtitleText;
    public List<SubtitleEntry> subtitles;

    void Update()
    {
        float t = (float)director.time;
        SubtitleEntry current = subtitles.Find(s => t >= s.startTime && t <= s.endTime);
        if (current != null)
        {
            subtitleText.text = current.text;
            subtitleText.gameObject.SetActive(true);
        }
        else
        {
            subtitleText.gameObject.SetActive(false);
        }
    }
}