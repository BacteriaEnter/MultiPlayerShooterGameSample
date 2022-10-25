using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ShooterGame : NetworkBehaviour
{
    private bool m_ClientGameOver = false;
    [SerializeField] private float m_TimeRemaining = 60;
    [SerializeField] private float roundTime = 60;
    [SerializeField] private float resetTime = 3;
    public TextMeshProUGUI gameTimerText;
    // Start is called before the first frame update
    void Start()
    {
        if (IsServer || IsHost)
        {
            List<NetworkClient> clients = (List<NetworkClient>)NetworkManager.Singleton.ConnectedClientsList;
            foreach (var client in clients)
            {
                SetClientScoreBoard(client);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (IsCurrentGameOver()) return;
        UpdateGameTimer();
    }

    private void UpdateGameTimer()
    {
        if (m_TimeRemaining > 0.0f)
        {
            m_TimeRemaining -= Time.deltaTime;

            if (IsServer && m_TimeRemaining <= 0.0f) // Only the server should be updating this
            {
                m_TimeRemaining = 0.0f;
                if (IsServer || IsHost)
                {
                    OpenScoreBoardClientRpc();

                }
            }

            if (m_TimeRemaining > 0.1f)
                gameTimerText.SetText("{0}", Mathf.FloorToInt(m_TimeRemaining));
        }
        else
        {
            if (IsServer || IsHost)
            {
                resetTime -= Time.deltaTime;
                if (resetTime <= 0)
                {
                    KillAllPlayerServerRpc();
                    SetRemainTimeClientRpc();
                    CloseScoreBoardClientRpc();
                    resetTime = 3f;
                }
            }
        }
    }


    [ClientRpc]
    void SetRemainTimeClientRpc()
    {
        m_TimeRemaining = roundTime;
    }

    private void SetClientScoreBoard(NetworkClient client)
    {
        string playerName = ServerGameNetPortal.Instance.GetPlayerName(client.ClientId);
        SetClientScoreBoardClientRpc(playerName);
    }



    [ServerRpc]
    void KillAllPlayerServerRpc()
    {
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerStatus>().HnaldeDeathClientRpc();
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            client.PlayerObject.GetComponent<PlayerStatus>().HnaldeDeathClientRpc();
        }


      
    }

    [ClientRpc]
    private void SetClientScoreBoardClientRpc(string playerName)
    {
        ScoreBoardUIManager.Instance.AddScoreBoard(playerName);
    }

    [ClientRpc]
    private void OpenScoreBoardClientRpc()
    {
        ScoreBoardUIManager.Instance.gameObject.SetActive(true);

    }

    [ClientRpc]
    private void CloseScoreBoardClientRpc()
    {
        ScoreBoardUIManager.Instance.gameObject.SetActive(false) ;
        ScoreBoardUIManager.Instance.ClearScore();
    }
    private bool ShouldStartCountDown()
    {
        if (m_TimeRemaining <= 0)
        {
            return false;
        }
        return true;
    }
    //private bool IsCurrentGameOver()
    //{
    //    if (IsServer)
    //        return isGameOver.Value;
    //    return m_ClientGameOver;
    //}
}
