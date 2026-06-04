using System.Runtime.InteropServices;
using Unity.AI.Navigation;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    #region Singleton
    private static LevelManager _instance;

    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
                FindFirstObjectByType<LevelManager>().Init();

            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
            Init();
        else if (_instance != this)
            Destroy(gameObject);
    }

    private void Init()
    {
        _instance = this;
    }
    #endregion

    private int                     currentTeddyBearValue;
    private GameObject              playerGameObject;
    private UIHandler               uiHandler;
    private IsometricFollowCamera   followCamera;
    private NavMeshSurface          navMeshSurface;

    public int TeddyBearCount => currentTeddyBearValue;
    public GameObject Player => playerGameObject;

    public void LevelGenerationFinished()
    {
        playerGameObject = GameObject.FindGameObjectWithTag("Player");
        if (playerGameObject == null) return;

        followCamera.SetCameraTarget(playerGameObject.transform);
        playerGameObject.GetComponent<ObstacleDetector>().SetCameraTransform(followCamera.transform);

        navMeshSurface?.BuildNavMesh();
        InitializePlayer();
    }

    /// <summary>
    /// Wires up player components to the UI and marks the player as ready to act.
    /// </summary>
    private void InitializePlayer()
    {
        PlayerHealth health = playerGameObject.GetComponent<PlayerHealth>();
        PlayerStamina stamina = playerGameObject.GetComponent<PlayerStamina>();
        PlayerInput input = playerGameObject.GetComponent<PlayerInput>();

        health.OnValueChanged += uiHandler.SetBarValue;
        stamina.OnValueChanged += uiHandler.SetBarValue;
        input.OnWeaponChange += uiHandler.ChangeWeaponIcon;

        uiHandler.SetBarValue(0, health.CurrentValue, health.MaxValue);
        uiHandler.SetBarValue(1, stamina.CurrentValue, stamina.MaxValue);
    }

    public void FinishLevel()
    {
        GameManager.Instance.IncreaseTeddyBear();

    }

    public void DisplayDeathScreen()
    {
        uiHandler.ShowDeathScreen();
    }
}
