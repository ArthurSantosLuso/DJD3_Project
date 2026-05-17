using System.Collections.Generic;
using System.Security;
using UnityEngine;

public class RoomBorder : MonoBehaviour
{
    [SerializeField] private List<GameObject> borders;
    [SerializeField] private int enemyCountToFinish;
    [SerializeField] private AgentPoolManager roomPool;

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerHealth>() != null)
        {
            foreach (GameObject go in borders)
            {
                go.SetActive(true);
            }
            roomPool.ShouldSpawn = true;
            GetComponent<Collider>().enabled = false;
        }
    }

    private void Update()
    {
        if (enemyCountToFinish == GameManager.Instance.EnemyDeadCount)
            OnRoomCleared();
        
    }

    public void OnRoomCleared()
    {
        foreach (GameObject go in borders)
        {
            go.SetActive(false);
        }
        roomPool.ShouldSpawn = false;
    }
}
