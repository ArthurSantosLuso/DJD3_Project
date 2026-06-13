using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image profileImage;
    [SerializeField] private GameObject continueIcon;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.04f;

    private Story currentStory;
    public bool IsDialoguePlaying { get; private set; }
    private Coroutine displayLineCoroutine;

    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
        dialoguePanel?.SetActive(false);
    }

    public void EnterDialogueMode(TextAsset inkJSON, Sprite whosTalkingImage = null)
    {
        GameManager.Instance.StopPlayerActions(false);
        if (profileImage)
        {
            profileImage.gameObject.SetActive(true);
            profileImage.sprite = whosTalkingImage;
        }
        currentStory = new Story(inkJSON.text);
        IsDialoguePlaying = true;
        dialoguePanel.SetActive(true);

        ContinueStory();
    }

    public void RequestNextLine()
    {
        // If still typing, skip to the end of the line
        if (displayLineCoroutine != null)
        {
            // skip logic
            return;
        }

        if (currentStory.canContinue)
        {
            Debug.Log("Escrevendo historia...");
            ContinueStory();
        }
        else
        {
            Debug.Log("Terminou a historia...");
            ExitDialogueMode();
        }
    }

    private void ContinueStory(Sprite profileImage = null)
    {
        string nextLine = currentStory.Continue();
        HandleTags(currentStory.currentTags);

        if (displayLineCoroutine != null) StopCoroutine(displayLineCoroutine);
        displayLineCoroutine = StartCoroutine(DisplayLine(nextLine));
    }

    private IEnumerator DisplayLine(string line)
    {
        dialogueText.text = "";
        continueIcon.SetActive(false);

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        continueIcon.SetActive(true);
        displayLineCoroutine = null;
    }

    public void ExitDialogueMode()
    {
        profileImage.gameObject.SetActive(false);
        IsDialoguePlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        GameManager.Instance.ActivatePlayerActions();
    }

    private void HandleTags(List<string> currentTags)
    {
        foreach (string tag in currentTags)
        {
            string[] splitTag = tag.Split(':');
            if (splitTag.Length != 2) continue;

            string key = splitTag[0].Trim().ToLower();
            string value = splitTag[1].Trim().ToLower();

            switch (key)
            {
                case "color":
                    // color logic
                    break;
            }
        }
    }
}