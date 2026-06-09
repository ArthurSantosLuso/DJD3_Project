using UnityEngine;
using System.Collections.Generic;

public class UpgradeGenerator : MonoBehaviour
{
    [SerializeField] private int                upgradeCount;
    [SerializeField] private List<UpgradeData>  avaliableUpgrades;
    [SerializeField] private GameObject         upgradesContainer;
    [SerializeField] private GameObject         upgradePrefab;

    private void Start()
    {
        if (GameManager.Instance.HasUpgradeAvaliable)
            GenerateNewUpgrades();
    }

    public void GenerateNewUpgrades()
    {
        // Aux list
        List<UpgradeData> upgrades = avaliableUpgrades;

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


}