using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private AudioClip _destroyClip;
    [SerializeField] private GameObject _particles;
    [SerializeField] private float speed = 5f;
    [SerializeField] private ulong ownerID;
    [SerializeField] ParticleSystem damageFX;
    [SerializeField] GameObject destroyFX;
    [SerializeField] AudioClip gunVoiceClip;
    [Range(0, 1)] public float gunVoiceAudioVolume = 0.5f;
    public void Init(ulong ownerID)
    {
        this.ownerID = ownerID;

    }
    private void OnEnable()
    {
        AudioSource.PlayClipAtPoint(gunVoiceClip, transform.TransformPoint(transform.position), gunVoiceAudioVolume);

    }

    public override void OnNetworkSpawn()
    {
        Invoke(nameof(DestroyBullet), 3);
    }

    private void DestroyBullet()
    {
        //AudioSource.PlayClipAtPoint(_destroyClip, transform.position);
        //Instantiate(_particles, transform.position, Quaternion.identity);
        //ClearID
        ownerID = 0;

        if (!NetworkObject.IsSpawned||!IsServer)
        {
            return;
        }
        NetworkObject.Despawn(true);

    }


    [ServerRpc]
    private void DeSpawnServerRpc()
    {
     
    }
    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, Time.deltaTime * speed);
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.CompareTag("Player"))
        {
            var playerHit = other.GetComponent<NetworkObject>();
            if (playerHit.OwnerClientId != OwnerClientId)
            {
                UpdateDamageFxServerRpc();
                if (IsOwner)
                {
                    UpdateHealthServerRpc(25, playerHit.OwnerClientId);
                }

            }
        }
        else if (other.CompareTag("Enviroment"))
        {
            var fx = Instantiate(destroyFX, transform.position, Quaternion.identity);
            fx.transform.up = -transform.forward;

            DestroyBullet();
        }

    }

    [ServerRpc]
    void UpdateDamageFxServerRpc()
    {
        updateDamageFxClientRpc();
    }

    [ClientRpc]
    void updateDamageFxClientRpc()
    {
        damageFX.transform.up = -transform.forward;
        damageFX.Play();
    }

    [ServerRpc]
    public void UpdateHealthServerRpc(int takeAwayPoint, ulong clientId)
    {
        var clientObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        var clientWithDamaged = clientObject.GetComponent<PlayerStatus>();

        if (clientWithDamaged != null && ownerID != clientId)//Damage check
        {
            bool isDead=clientWithDamaged.TakeDamage(takeAwayPoint);
            if (isDead)
            {
                string name = ServerGameNetPortal.Instance.GetPlayerName(ownerID);
                UpdatePlayerScoreClientRpc(name);
            }
        }   
    }


    [ClientRpc]
    void UpdatePlayerScoreClientRpc(string name)
    {
        
        ScoreBoardUIManager.Instance.UpdateScoreBoard(name);
    }
}
