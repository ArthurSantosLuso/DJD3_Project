using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class UIMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private GameObject menuUI;
    [SerializeField] private PlayerInput playerInput;

    public CinemachineCamera currentCamera;
    [SerializeField] private CinemachineCamera menuCamera;
    [SerializeField] private CinemachineCamera gameplayCamera;

    void Start()
    {
        gameplayUI.SetActive(false);
        menuUI.SetActive(true);
        playerInput.enabled = false;
        currentCamera.Priority++;
    }

    public void UpdateCamera(CinemachineCamera target)
    {
        currentCamera.Priority--;
        currentCamera = target;
        currentCamera.Priority++;
    }

    public void StartGame()
    {
        gameplayUI.SetActive(true);
        menuUI.SetActive(false);
        playerInput.enabled = true;
        menuCamera.Priority = 0;
        gameplayCamera.Priority = 10;
    }
}