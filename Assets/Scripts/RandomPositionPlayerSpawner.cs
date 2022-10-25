using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RandomPositionPlayerSpawner : MonoBehaviour
{
    NetworkManager m_NetworkManager;
    [SerializeField] List<Transform> spawnPos=new List<Transform>();
     
    public Vector3 GetNextSpawnPosition()
    {
        var index = Random.Range(0, spawnPos.Count);
        return spawnPos[index].position;
    }

    private void Awake()
    {
        var networkManager = FindObjectOfType<NetworkManager>();
        //networkManager.ConnectionApprovalCallback += ConnectionApprovalWithRandomSpawnPos;
    }

    void ConnectionApprovalWithRandomSpawnPos(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // Here we are only using ConnectionApproval to set the player's spawn position. Connections are always approved.
        response.CreatePlayerObject = true;
        response.Position = GetNextSpawnPosition();
        response.Rotation = Quaternion.identity;
        response.Approved = true;
    }
}
