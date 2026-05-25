using UnityEngine;
using Unity.AI.Navigation;

public class GameManager : MonoBehaviour
{
    #region Singleton stuff
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

    [SerializeField]
    private UIHandler uiHandler;

    [SerializeField]
    private GameObject player;

    public UIHandler UIHandler => uiHandler;
    public GameObject Player => player;
    public int EnemyDeadCount { get; set; }
    public bool CanPlayerAct { get; private set; }

    private void Start()
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

    // Not in use. 
    public void ChangeUIBarValue(int barIdx, float currentValue, float maxValue)
    {
        uiHandler.SetBarValue(barIdx, currentValue, maxValue);
    }

    public void DisplayDeathScreen()
    {
        uiHandler.ShowDeathScreen();
    }

    public void StopPlayerActions()
    {
        CanPlayerAct = false;
        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponent<Character>().enabled = false;
        player.GetComponent<PlayerStamina>().enabled = false;
        player.GetComponent<RotateToFaceMouse>().enabled = false;
    }

    public void ActivePlayerActions()
    {
        CanPlayerAct = true;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<Character>().enabled = true;
        player.GetComponent<PlayerStamina>().enabled = true;
        player.GetComponent<RotateToFaceMouse>().enabled = true;
    }
}
