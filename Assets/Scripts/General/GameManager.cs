using UnityEngine;
using Unity.AI.Navigation;

/*
This script handles:
Global game state — the player reference, UI wiring, NavMesh baking, and player action toggling.
Acts as the central access point for other systems that need the player or UI handler.

NOTE: The player is found via tag in LateUpdate and then the setup runs once.
This is a workaround for the player being spawned at runtime by LevelGenerator.
If the spawn order ever becomes deterministic, a direct reference or an event would be cleaner.
*/

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
                FindFirstObjectByType<GameManager>().Init();

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

    [SerializeField] private UIHandler uiHandler;
    [SerializeField] private IsometricFollowCamera followCamera;

    private GameObject player;

    [SerializeField] private int teddyBearCount;
    [SerializeField] private NavMeshSurface navMeshSurface;

    public int TeddyBearCount => teddyBearCount;
    public UIHandler UIHandler => uiHandler;
    public GameObject Player => player;
    public int EnemyDeadCount { get; set; }
    public bool CanPlayerAct { get; private set; }

    private void LateUpdate()
    {
        if (player != null) return;

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        followCamera.SetCameraTarget(player.transform);
        player.GetComponent<ObstacleDetector>().SetCameraTransform(followCamera.transform);

        navMeshSurface.BuildNavMesh();
        InitializePlayer();
    }

    /// <summary>
    /// Wires up player components to the UI and marks the player as ready to act.
    /// </summary>
    private void InitializePlayer()
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        PlayerStamina stamina = player.GetComponent<PlayerStamina>();
        PlayerInput input = player.GetComponent<PlayerInput>();

        health.OnValueChanged += uiHandler.SetBarValue;
        stamina.OnValueChanged += uiHandler.SetBarValue;
        input.OnWeaponChange += uiHandler.ChangeWeaponIcon;

        uiHandler.SetBarValue(0, health.CurrentValue, health.MaxValue);
        uiHandler.SetBarValue(1, stamina.CurrentValue, stamina.MaxValue);

        CanPlayerAct = true;
    }

    public void DisplayDeathScreen()
    {
        uiHandler.ShowDeathScreen();
    }

    /// <summary>
    /// Disables all player controlled components and pauses the game.
    /// </summary>
    public void StopPlayerActions()
    {
        CanPlayerAct = false;
        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponent<Character>().enabled = false;
        player.GetComponent<PlayerStamina>().enabled = false;
        player.GetComponent<RotateToFaceMouse>().enabled = false;

        Time.timeScale = 0f;
    }

    /// <summary>
    /// Re-enables all player controlled components and resumes the game.
    /// </summary>
    public void ActivatePlayerActions()
    {
        CanPlayerAct = true;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<Character>().enabled = true;
        player.GetComponent<PlayerStamina>().enabled = true;
        player.GetComponent<RotateToFaceMouse>().enabled = true;

        Time.timeScale = 1f;
    }
}