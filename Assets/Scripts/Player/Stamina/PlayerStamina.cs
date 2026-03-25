using UnityEngine;

public class PlayerStamina : ValueBase
{
    private void Start()
    {
        UIManager.Instance.RegisterStamina(this);
    }
}
