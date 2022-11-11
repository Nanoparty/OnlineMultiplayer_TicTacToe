using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
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
        Single.onClick.AddListener(() =>
        {
            MainMenu.SetActive(false);
            SingleMenu.SetActive(true);
            SingleMenu.GetComponentInChildren<Button>().onClick.AddListener(() => {
                player1 = SingleMenu.GetComponentInChildren<TMP_InputField>().text;
                UpdateData();
                SceneManager.LoadScene("Match", LoadSceneMode.Single);
            });
        });
        LocalMulti.onClick.AddListener(() =>
        {
            MainMenu.SetActive(false);
            LocalMenu.SetActive(true);
            LocalMenu.GetComponentInChildren<Button>().onClick.AddListener(() => {
                player1 = LocalMenu.transform.GetChild(1).GetComponent<TMP_InputField>().text;
                player2 = LocalMenu.transform.GetChild(2).GetComponent<TMP_InputField>().text;
                UpdateData();
                SceneManager.LoadScene("Match", LoadSceneMode.Single);
            });
        });
        Host.onClick.AddListener(() =>
        {
            MainMenu.SetActive(false);
            HostMenu.SetActive(true);
            HostMenu.GetComponentInChildren<Button>().onClick.AddListener(() => {
                player1 = HostMenu.GetComponentInChildren<TMP_InputField>().text;
                StartLocalGame(player1);
            });
        });
        Join.onClick.AddListener(() =>
        {
            MainMenu.SetActive(false);
            JoinMenu.SetActive(true);
            JoinMenu.GetComponentInChildren<Button>().onClick.AddListener(() => {
                player2 = JoinMenu.transform.GetChild(1).GetComponent<TMP_InputField>().text;
                ipAddress = JoinMenu.transform.GetChild(2).GetComponent<TMP_InputField>().text;
                JoinLocalGame(player2);
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

    public void StartLocalGame(string name)
    {
        if (NetworkManager.Singleton.StartHost())
        {
            Data.AddPlayerName(NetworkManager.Singleton.LocalClientId, name);
            //Data.AddPlayerName( = name;
            SceneTransitionHandler.sceneTransitionHandler.RegisterCallbacks();
            SceneTransitionHandler.sceneTransitionHandler.SwitchScene("Lobby");
        }
        else
        {
            Debug.LogError("Failed to start host.");
        }
    }

    public void JoinLocalGame(string name)
    {
        if (NetworkManager.Singleton.StartClient())
        {
            //Debug.Log("LocalClientId:" + NetworkManager.Singleton.LocalClientId);
            //Data.AddPlayerName(NetworkManager.Singleton.LocalClientId, name);
            Data.localName = name;
        }
        else
        {
            Debug.LogError("Failed to start client.");
        }
    }
}
