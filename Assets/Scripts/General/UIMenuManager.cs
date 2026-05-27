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