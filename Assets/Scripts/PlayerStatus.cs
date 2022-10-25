using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerStatus : NetworkBehaviour
{
    PlayerShooting playerShooting;
    public NetworkVariable<int> networkPlayerHealth = new(1000, NetworkVariableReadPermission.Everyone);
    public GameObject characterModel;
    public GameObject weaponModel;
    private NetworkVariable<FixedString32Bytes> displayName = new NetworkVariable<FixedString32Bytes>();
    [SerializeField] private TMP_Text displayNameText;
    [SerializeField] Camera minimap;
    [SerializeField] GameObject deathFX;
    [SerializeField] AudioClip deathVoiceClip;
    [Range(0, 1)] public float deathVoiceAudioVolume = 0.5f;

    string tempName;
    //Health=networkPlayerHealth.Value;
    UIManager uIManager;
    private void Awake()
    {
        playerShooting = GetComponent<PlayerShooting>();
    }

    private void OnEnable()
    {
        if (IsOwner)
        {
            uIManager.UpdateHpStatus(networkPlayerHealth.Value);
        }
    }
    private void Start()
    {
        uIManager?.UpdateHpStatus(networkPlayerHealth.Value);
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            tempName = ServerGameNetPortal.Instance.GetPlayerName(OwnerClientId);
            minimap.GetComponent<MiniMap>().playerTransform = transform;
            minimap.gameObject.SetActive(true);
            SetPlayerNameServerRpc();
            displayNameText.text = displayName.Value.ToString();
            uIManager = FindObjectOfType<UIManager>();
        }
        else
        {
            tempName = ServerGameNetPortal.Instance.GetPlayerName(OwnerClientId);
            SetPlayerNameServerRpc();
            displayNameText.text = displayName.Value.ToString();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc()
    {
        displayName.Value = tempName;
    }
    private void Update()
    {
        uIManager?.UpdateHpStatus(networkPlayerHealth.Value);
    }

    public bool TakeDamage(int damage)
    {
        if (playerShooting.currenState.Value == playerState.Dead)
            return true;
        networkPlayerHealth.Value -= damage;

        if (networkPlayerHealth.Value <= 0)
        {
            networkPlayerHealth.Value = 0;
            playerShooting.currenState.Value = playerState.Dead;
            HnaldeDeathClientRpc();
            return true;
        }
        return false;
    }

    [ClientRpc]
    public void HnaldeDeathClientRpc()
    {
        Vector3 spawnPosition = FindObjectOfType<RandomPositionPlayerSpawner>().GetNextSpawnPosition();
        Instantiate(deathFX, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(deathVoiceClip, transform.TransformPoint(transform.position), deathVoiceAudioVolume);
        transform.position = spawnPosition;
        characterModel.SetActive(false);
        weaponModel.SetActive(false);
        Invoke("SpawnPlayer", 3);
        playerShooting.InitWeapon();
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetStatusServerRpc()
    {
        networkPlayerHealth.Value = 100;
        playerShooting.currenState.Value = playerState.Normal;
    }
    public void SpawnPlayer()
    {

        SetStatusServerRpc();

        characterModel.SetActive(true);
        weaponModel.SetActive(true);
    }

}
