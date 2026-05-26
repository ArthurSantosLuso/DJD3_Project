using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.UI;

public class UIMenuManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject menuUI;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject weaponDisplay;
    [SerializeField] private GameObject staminaBar;
    [SerializeField] private GraphicRaycaster gameplayRaycaster;
    [SerializeField] private GameObject renderingCanvas;

    [Header("Input")]
    [SerializeField] private GameObject playerObject;

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

        // UI
        menuUI.SetActive(true);
        healthBar.SetActive(false);
        weaponDisplay.SetActive(false);
        staminaBar.SetActive(false);
        gameplayRaycaster.enabled = false;
        playerObject.SetActive(false);
        renderingCanvas.SetActive(false);
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

        // UI
        menuUI.SetActive(false);
        healthBar.SetActive(true);
        weaponDisplay.SetActive(true);
        staminaBar.SetActive(true);
        gameplayRaycaster.enabled = true;
        playerObject.SetActive(true);
        renderingCanvas.SetActive(true);
    }
}