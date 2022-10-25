using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class SpawnHandler : NetworkBehaviour
{
    [SerializeField] private GameObject playerObject;
    [SerializeField] private ulong clientId;
    private void Start()
    {
        
        clientId = NetworkManager.Singleton.LocalClientId;
        SpawnPlayerServerRpc(clientId);
        FindObjectOfType<NetworkObjectPool>().InitializePool();
    }


    [ServerRpc(RequireOwnership =false)]
    public void SpawnPlayerServerRpc(ulong clientId)
    {
        Debug.Log("Spawned");
        var player = Instantiate(playerObject, transform.position, Quaternion.identity);
        
        player.transform.position = FindObjectOfType<RandomPositionPlayerSpawner>().GetNextSpawnPosition();
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        //GameObject.FindGameObjectWithTag("MiniMapCamera").GetComponent<MiniMap>().playerTransform = player.transform;

    }
   
}
