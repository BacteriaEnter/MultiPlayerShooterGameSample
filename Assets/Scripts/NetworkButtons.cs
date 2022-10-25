using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkButtons : MonoBehaviour
{
    [SerializeField] UnityTransport transport;
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient&& !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host"))
            {
                NetworkManager.Singleton.StartHost();
            }
            if (GUILayout.Button("Server"))
            {
                NetworkManager.Singleton.StartServer();
            }
            if (GUILayout.Button("Client"))
            {
                NetworkManager.Singleton.StartClient();
            }
   
        }
        else

        {
            if (GUILayout.Button("Send"))
            {
                SendMessage();
            }
        }
        GUILayout.EndArea();
    }

    void SendMessage()
    {
        //var message = Encoding.UTF8.GetBytes("11111");
        //ArraySegment<byte> segement = new ArraySegment<byte>(message);
        //transport.Send(transport.ServerClientId, segement, NetworkDelivery.Reliable);
        //FastBufferWriter writer = new FastBufferWriter(Marshal.SizeOf()+overh, Allocator.Temp);
        //NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("11111", transport.ServerClientId)
    }
}
