using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIMenuManager : MonoBehaviour
{
    [Header("Start Disable")]
    [SerializeField] private List<GameObject> objs;
    [Header("Start Enable")]
    [SerializeField] private List<GameObject> objsActive;
    [Header("UI")]
    [SerializeField] private GraphicRaycaster gameplayRaycaster;

    [Header("Cameras")]
    public CinemachineCamera currentCamera;
    [SerializeField] private CinemachineCamera menuCamera;
    [SerializeField] private CinemachineCamera gameplayCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineCamera optionsCamera;
    [SerializeField] private RenderTexture pixelRenderTexture;

    [Header("First Run")]
    [SerializeField] private DialogueInteractable uncleBenDialogueInteractable;

    private CinemachineBrain cameraBrain;

    void Awake()
    {
        // Cameras
        menuCamera.Priority = 100;
        gameplayCamera.Priority = 0;
        optionsCamera.Priority = 0;
        currentCamera = menuCamera;
        cinemachineBrain.LensModeOverride.Enabled = false;
        mainCamera.targetTexture = null;

        gameplayRaycaster.enabled = false;

        foreach (var obj in objs)
        {
            obj.SetActive(false);
        }

        foreach (var obj in objsActive)
        {
            obj.SetActive(true);
        }

        cameraBrain = mainCamera.GetComponent<CinemachineBrain>();
    }

    private void Update()
    {
        if (cameraBrain.IsBlending)
        {
            var currentBlend = cameraBrain.ActiveBlend;

            if (currentBlend != null)
            {
                float blendPercent = currentBlend.TimeInBlend / currentBlend.Duration;

                if (blendPercent >= 0.92f)
                {
                    if (GameManager.Instance.TeddyBearCount == 0)
                    {
                        uncleBenDialogueInteractable.TryInteract();
                    }
                    else
                    {
                        GameManager.Instance.ActivatePlayerActions();
                        gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public void UpdateCamera(CinemachineCamera target)
    {
        currentCamera.Priority = 0;
        currentCamera = target;
        currentCamera.Priority = 100;

    }

    public void StartGame()
    {
        // Cameras
        currentCamera.Priority = 0;
        gameplayCamera.Priority = 100;
        cinemachineBrain.LensModeOverride.Enabled = true;
        mainCamera.targetTexture = pixelRenderTexture; 

        gameplayRaycaster.enabled = true;

        foreach (var obj in objs)
        {
            obj.SetActive(true);
        }

        foreach (var obj in objsActive)
        {
            obj.SetActive(false);
        }
    }
}