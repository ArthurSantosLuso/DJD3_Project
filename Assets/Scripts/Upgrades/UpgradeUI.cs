using UnityEngine;
using System.Collections;
using TMPro;
using System;
using UnityEngine.UI;


public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI    upgradeTitle;
    [SerializeField] private Button             selectButton;

    private UpgradeData data;

    public void Initialize(UpgradeData data)
    {
        this.data = data;
        if (upgradeTitle != null) upgradeTitle.text = data.UpgradeName;
        if (selectButton != null) selectButton.onClick.AddListener(OnPlayerSelectUpgrade);
    }

    private void OnPlayerSelectUpgrade()
    {
        GameManager.Instance.AddPlayerUpgrade(data);
    }
}