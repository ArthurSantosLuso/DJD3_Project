using UnityEngine;

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
}
