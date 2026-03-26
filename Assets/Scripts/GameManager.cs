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

    private void Start()
    {
        player.GetComponent<PlayerHealth>().OnValueChanged += uiHandler.SetBarValue;
        player.GetComponent<PlayerStamina>().OnValueChanged += uiHandler.SetBarValue;
    }


    public void ChangeUIBarValue(int barIdx, float currentValue, float maxValue)
    {
        uiHandler.SetBarValue(barIdx, currentValue, maxValue);
    }
}
