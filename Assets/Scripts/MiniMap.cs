using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    public Transform playerTransform;

    private void Start()
    {
        //playerTransform = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
     
    }
    private void LateUpdate()
    {
        if (playerTransform = null)
        {
            transform.position = new Vector3(playerTransform.position.x, 40, playerTransform.position.z);
        }
    }
}
