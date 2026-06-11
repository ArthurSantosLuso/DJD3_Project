using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine.SceneManagement;

/*
This script handles:
Player action toggling and storing game important data such as Teddy Bear count (scaling factor) and player upgrades.
Is universal for every scene in the game.
*/

public class GameManager : MonoBehaviour
{
    #region Singleton
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
        DontDestroyOnLoad(gameObject);
        LoadGame(allUpgrades);
    }
    #endregion

    private void OnEnable()
    {
    }

    [SerializeField] private int teddyBearCount;
    [SerializeField] private int availableUpgrades;
    [SerializeField] private List<UpgradeData> allUpgrades;

    private List<UpgradeData> playerUpgrades = new();

    private const string SAVE_KEY = "SaveData";

    public List<UpgradeData> PlayerUpgrades => playerUpgrades;
    public int TeddyBearCount => teddyBearCount;
    public bool HasUpgradeAvaliable => availableUpgrades > 0;
    public int AvaliableUpgrades => availableUpgrades;
    public bool CanPlayerAct { get; private set; }

    public void IncreaseTeddyBear() => teddyBearCount++;

    public void AddPlayerUpgrade(UpgradeData upgrade)
    {
        playerUpgrades.Add(upgrade);
        availableUpgrades--;
    }

    public void AddAvailableUpgrade() => availableUpgrades++;

    public void SaveGame()
    {
        SaveData save = new SaveData
        {
            teddyBearCount = teddyBearCount,
            availableUpgrades = availableUpgrades,
            playerUpgradesIDs = playerUpgrades.ConvertAll(s => s.ID)

        };

        string json = JsonUtility.ToJson(save);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public void LoadGame(List<UpgradeData> allUpgrades)
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY)) return;

        string json = PlayerPrefs.GetString(SAVE_KEY);
        SaveData save = JsonUtility.FromJson<SaveData>(json);

        teddyBearCount = save.teddyBearCount;
        availableUpgrades = save.availableUpgrades;

        playerUpgrades.Clear();
        foreach (string id in save.playerUpgradesIDs)
        {
            UpgradeData match = allUpgrades.Find(s => s.ID == id);
            if (match != null) playerUpgrades.Add(match);
        }
    }

    #region Game State - Pause

    /// <summary>
    /// Disables all player controlled components and pauses the game.
    /// Look for game object via FindGameObjectWithTag("Player").
    /// </summary>
    public void StopPlayerActions(bool shouldStopTime = true)
    {
        CanPlayerAct = false;
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        foreach (var pausable in player.GetComponents<PausableMonoBehaviour>())
            pausable.Pause();

        if (shouldStopTime) Time.timeScale = 0f;
    }

    /// <summary>
    /// Disables all player controlled components and pauses the game.
    /// </summary>
    public void StopPlayerActions(GameObject player, bool shouldStopTime = true)
    {
        CanPlayerAct = false;

        foreach (var pausable in player.GetComponents<PausableMonoBehaviour>())
            pausable.Pause();

        if (shouldStopTime) Time.timeScale = 0f;
    }

    /// <summary>
    /// Re-enables all player controlled components and resumes the game.
    /// Look for game object via FindGameObjectWithTag("Player").
    /// </summary>
    public void ActivatePlayerActions()
    {
        CanPlayerAct = true;
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        foreach (var pausable in player.GetComponents<PausableMonoBehaviour>())
            pausable.UnPause();

        Time.timeScale = 1f;
    }

    /// <summary>
    /// Re-enables all player controlled components and resumes the game.
    /// </summary>
    public void ActivatePlayerActions(GameObject player)
    {
        CanPlayerAct = true;

        foreach (var pausable in player.GetComponents<PausableMonoBehaviour>())
            pausable.UnPause();

        Time.timeScale = 1f;
    }

    #endregion
}