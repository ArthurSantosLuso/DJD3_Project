using UnityEngine;
using System.Collections.Generic;

public class UpgradeGenerator : MonoBehaviour
{
    [SerializeField] private int                upgradeCount;
    [SerializeField] private List<UpgradeData>  avaliableUpgrades;
    [SerializeField] private GameObject         upgradesContainer;
    [SerializeField] private GameObject         upgradePrefab;
    [SerializeField] private GameObject         noUpgradeText;
    [SerializeField] private GameObject         upgradePanel;

    private void OnEnable()
    {
        UpgradeUI.OnUpgradeSelected += HandleUpgradeSelected;

        if (GameManager.Instance.HasUpgradeAvaliable && upgradesContainer.transform.childCount == 0)
            GenerateNewUpgrades();
        else if (!GameManager.Instance.HasUpgradeAvaliable) noUpgradeText.SetActive(true);
    }

    private void OnDisable()
    {
        UpgradeUI.OnUpgradeSelected -= HandleUpgradeSelected;
    }

    public void GenerateNewUpgrades()
    {
        noUpgradeText.SetActive(false);
        // Aux list
        List<UpgradeData> upgrades = new(avaliableUpgrades);

        for (int i = 0; i < upgradeCount; i++)
        {
            // Pick one random upgrade
            int idx = Random.Range(0, upgrades.Count);
            // Get upgrade data
            UpgradeData data = upgrades[idx];
            // Remove upgrade from aux list to avoid repeated upgrades
            upgrades.Remove(data);

            // Instantiate upgrade UI prefab to upgrades container
            GameObject obj = Instantiate(upgradePrefab, upgradesContainer.transform);

            // Initialize UI
            obj.GetComponent<UpgradeUI>().Initialize(data);
        }
    }

    private void HandleUpgradeSelected()
    {
        // Clear all upgrade UI cards
        foreach (Transform child in upgradesContainer.transform)
            Destroy(child.gameObject);

        // Close the canvas and restore player control
        upgradePanel.SetActive(false);
        GameManager.Instance.ActivatePlayerActions();
    }
}