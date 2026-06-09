
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Scriptable Objects/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public enum UpgradeTypes { Life, ShotgunDamage, AxeDamage, AxeAbilityCooldown, ShotgunAbilityCooldown, AxeAbilityDamage, ShotgunAbilityDamage }

    [SerializeField] private string         upgradeName;
    [SerializeField] private int            amountToChange;
    [SerializeField] private UpgradeTypes   upgradeType;

    public string UpgradeName => upgradeName;
    public int AmountToChange => amountToChange;
    public UpgradeTypes UpgradeType => upgradeType;
}
