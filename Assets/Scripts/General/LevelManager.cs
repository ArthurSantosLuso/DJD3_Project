using System.Runtime.InteropServices;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.InputSystem.DualShock;

[RequireComponent(typeof(LevelGenerator))]
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

    [SerializeField] private IsometricFollowCamera      followCamera;
    [SerializeField] private UIHandler                  uiHandler;
    [SerializeField] private LevelGenerator             levelGenerator;
    [SerializeField] private NavMeshSurface             navMeshSurface;
    [SceneDropdown]
    [SerializeField] private string                     sceneToOpenWhenLevelFinished;
    [SerializeField] private ScreenFader                screenFader;   

    private int                     currentTeddyBearValue;
    private GameObject              playerGameObject;

    public int TeddyBearCount => currentTeddyBearValue;
    public GameObject Player => playerGameObject;

    private void Start()
    {
        if (levelGenerator == null) levelGenerator = GetComponent<LevelGenerator>();
        currentTeddyBearValue = GameManager.Instance.TeddyBearCount;
        levelGenerator.GenerateLevel();
    }

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
        Character playerCharacter = playerGameObject.GetComponent<Character>();
        PlayerHealth health = playerGameObject.GetComponent<PlayerHealth>();
        PlayerStamina stamina = playerGameObject.GetComponent<PlayerStamina>();
        PlayerInput input = playerGameObject.GetComponent<PlayerInput>();

        health.OnValueChanged += uiHandler.SetBarValue;
        stamina.OnValueChanged += uiHandler.SetBarValue;
        input.OnWeaponChange += uiHandler.ChangeWeaponIcon;

        uiHandler.SetBarValue(0, health.CurrentValue, health.MaxValue);
        uiHandler.SetBarValue(1, stamina.CurrentValue, stamina.MaxValue);

        IAmmoProvider shotgun = playerGameObject.GetComponentInChildren<IAmmoProvider>(true);

        if (shotgun != null)
        {
            shotgun.OnAmmoChanged += uiHandler.UpdateAmmoText;
            uiHandler.UpdateAmmoText(shotgun.CurrentAmmo, shotgun.MaxAmmo);
        }
    }

    public void FinishLevel()
    {
        GameManager.Instance.IncreaseTeddyBear();
        screenFader?.FadeAndLoad(sceneToOpenWhenLevelFinished, 1f);
    }

    public void DisplayDeathScreen()
    {
        uiHandler.ShowDeathScreen();
    }
}
