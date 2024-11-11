using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkButtons : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.BeginHorizontal(GUILayout.Width(300), GUILayout.Height(50));

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
            if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        }

        GUILayout.EndHorizontal();
    }
}
