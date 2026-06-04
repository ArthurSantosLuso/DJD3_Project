using UnityEngine;

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
    }
    #endregion

    [SerializeField] private int teddyBearCount;

    public int TeddyBearCount => teddyBearCount;
    public bool CanPlayerAct { get; private set; }

    public void IncreaseTeddyBear() => teddyBearCount++;

    #region Game State - Pause

    /// <summary>
    /// Disables all player controlled components and pauses the game.
    /// Look for game object via FindGameObjectWithTag("Player").
    /// </summary>
    public void StopPlayerActions()
    {
        CanPlayerAct = false;
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        foreach (var pausable in player.GetComponents<PausableMonoBehaviour>())
            pausable.Pause();

        Time.timeScale = 0f;
    }

    /// <summary>
    /// Disables all player controlled components and pauses the game.
    /// </summary>
    public void StopPlayerActions(GameObject player)
    {
        CanPlayerAct = false;

        foreach (var pausable in player.GetComponents<PausableMonoBehaviour>())
            pausable.Pause();

        Time.timeScale = 0f;
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