using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField displayNameInputField;

    [Header("Panel")]
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField accountInput;
    [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private TMP_InputField portInput;

    [SerializeField] RegisterController register;
    [SerializeField] LoginController login;

    private void Start()
    {
        PlayerPrefs.GetString("PlayerName");
    }

    public void OnHostClicked()
    {
        PlayerPrefs.SetString("PlayerName", displayNameInputField.text);
        var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        ushort.TryParse(portInput.text, out ushort result);
        transport.SetConnectionData(ipInput.text, result);
        GameNetPortal.Instance.StartHost();
    }

    public void OnClientClicked()
    {
        PlayerPrefs.SetString("PlayerName", displayNameInputField.text);
        var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        ushort.TryParse(portInput.text, out ushort result);
        transport.SetConnectionData(ipInput.text, result);
        ClientGameNetPortal.Instance.StartClient();
    }

    public void OnRegisterClicked()
    {
        register.Register(accountInput.text, passwordInput.text);
    }

    public void OnLoginClicked()
    {
        login.Login(accountInput.text, passwordInput.text);
    }
}

