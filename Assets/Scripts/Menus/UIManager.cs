using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Slider lifeBar;
    [SerializeField]
    private Slider staminaBar;

    private static UIManager _instance;

    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
                FindFirstObjectByType<UIManager>().Init();

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

    public void SetHealthBarValue(float hpValue, float maxHpValue)
    {
        lifeBar.value = hpValue / maxHpValue;
    }

    public void SetStaminaBarValue(float staminaValue, float maxStaminaValue)
    {
        staminaBar.value = staminaValue / maxStaminaValue;
    }

    public void RegisterHealth(PlayerHealth health)
    {
        health.OnValueChanged += SetHealthBarValue;

        // inicializa UI
        SetHealthBarValue(health.CurrentValue, health.MaxValue);
    }

    public void RegisterStamina(PlayerStamina stamina)
    {
        stamina.OnValueChanged += SetStaminaBarValue;

        // inicializa UI
        SetStaminaBarValue(stamina.CurrentValue, stamina.MaxValue);
    }
}
