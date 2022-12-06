using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Button Single;
    public Button LocalMulti;
    public Button Host;
    public Button Join;

    public GameObject MainMenu;
    public GameObject SingleMenu;
    public GameObject LocalMenu;
    public GameObject HostMenu;
    public GameObject JoinMenu;

    public string player1 = "Player 1";
    public string player2 = "Player 2";

    public string status = "Local";
    public string ipAddress = "0.0.0.0";

    private void Start()
    {
        AudioManager.am.PlayMenuMusic();
        Single.onClick.AddListener(() =>
        {
            AudioManager.am.PlayClick1();
            MainMenu.SetActive(false);
            SingleMenu.SetActive(true);
            SingleMenu.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.am.PlayClick1();
                player1 = SingleMenu.GetComponentInChildren<TMP_InputField>().text;
                Data.localName = player1;
                //UpdateData();
                SceneManager.LoadScene("LocalMatch", LoadSceneMode.Single);
            });
            SingleMenu.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                AudioManager.am.PlayClick1();
                MainMenu.SetActive(true);
                SingleMenu.SetActive(false);
            });
        });
        LocalMulti.onClick.AddListener(() =>
        {
            AudioManager.am.PlayClick1();
            MainMenu.SetActive(false);
            LocalMenu.SetActive(true);
            LocalMenu.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.am.PlayClick1();
                player1 = LocalMenu.transform.GetChild(1).GetComponent<TMP_InputField>().text;
                player2 = LocalMenu.transform.GetChild(2).GetComponent<TMP_InputField>().text;
                Data.player1 = player1;
                Data.player2 = player2;
                //UpdateData();
                SceneManager.LoadScene("LocalMultiplayer", LoadSceneMode.Single);
            });
            LocalMenu.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() =>
            {
                AudioManager.am.PlayClick1();
                MainMenu.SetActive(true);
                LocalMenu.SetActive(false);
            });
        });
        Host.onClick.AddListener(() =>
        {
            AudioManager.am.PlayClick1();
            MainMenu.SetActive(false);
            HostMenu.SetActive(true);
            HostMenu.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.am.PlayClick1();
                player1 = HostMenu.GetComponentInChildren<TMP_InputField>().text;
                StartLocalGame(player1);
            });
            HostMenu.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                AudioManager.am.PlayClick1();
                MainMenu.SetActive(true);
                HostMenu.SetActive(false);
            });
        });
        Join.onClick.AddListener(() =>
        {
            AudioManager.am.PlayClick1();
            MainMenu.SetActive(false);
            JoinMenu.SetActive(true);
            JoinMenu.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.am.PlayClick1();
                player2 = JoinMenu.transform.GetChild(1).GetComponent<TMP_InputField>().text;
                ipAddress = JoinMenu.transform.GetChild(2).GetComponent<TMP_InputField>().text;
                JoinLocalGame(player2, ipAddress);
            });
            JoinMenu.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() =>
            {
                AudioManager.am.PlayClick1();
                MainMenu.SetActive(true);
                JoinMenu.SetActive(false);
            });
        });
    }

    private void UpdateData()
    {
        Data.status = status;
        Data.ipAddress = ipAddress;
        Data.player1 = player1;
        Data.player2 = player2;
    }

    public async void StartLocalGame(string name)
    {
        HostMenu.transform.GetChild(2).GetComponent<Button>().onClick.RemoveAllListeners();
        HostMenu.transform.GetChild(3).GetComponent<Button>().onClick.RemoveAllListeners();

        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsAuthorized)
        {
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log($"Signed in as {AuthenticationService.Instance.PlayerId}");
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        
        CreateRelay(name);
    }

    public async void CreateRelay(string name)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Join Code:" + joinCode);
            Data.joinCode = joinCode;

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            if (NetworkManager.Singleton.StartHost())
            {
                
                Debug.Log("Starting Host");
                Data.AddPlayerName(NetworkManager.Singleton.LocalClientId, name);
                SceneTransitionHandler.sceneTransitionHandler.RegisterCallbacks();
                SceneTransitionHandler.sceneTransitionHandler.SwitchScene("Lobby");
            }
            else
            {
                Debug.LogError("Failed to start host.");
            }
        }
        catch (RelayServiceException e)
        {
            Debug.Log("Create Relay Failed.\n" + e);
        }
    }

    public async void JoinLocalGame(string name, string joinCode)
    {
        JoinMenu.transform.GetChild(3).GetComponent<Button>().onClick.RemoveAllListeners();
        JoinMenu.transform.GetChild(4).GetComponent<Button>().onClick.RemoveAllListeners();

        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsAuthorized)
        {
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log($"Signed in as {AuthenticationService.Instance.PlayerId}");
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        Data.joinCode = joinCode;

        JoinRelay(joinCode, name);
        
    }

    private async void JoinRelay(string joinCode, string name)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            if (NetworkManager.Singleton.StartClient())
            { 
                Data.localName = name;
            }
            else
            {
                Debug.LogError("Failed to start client.");
            }
        }
        catch (RelayServiceException e)
        {
            Debug.Log("Join Relay Failed.\n" + e);
        }
    }

    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }
}
