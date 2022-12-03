using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField]
    private string inGameSceneName = "Match";

    [SerializeField]
    private int minimumPlayerCount = 2;
    [SerializeField]
    private int maximumPlayerCount = 2;

    public TMP_Text lobbyText;
    private bool allPlayersInLobby;

    private Dictionary<ulong, bool> clientsInLobby;
    private Dictionary<ulong, string> clientNames;
    private string UserLobbyStatusText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        clientsInLobby = new Dictionary<ulong, bool>();
        clientNames = new Dictionary<ulong, string>();

        clientsInLobby.Add(NetworkManager.LocalClientId, false);

        if (IsServer)
        {
            allPlayersInLobby = false;
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            SceneTransitionHandler.sceneTransitionHandler.OnClientLoadedScene += ClientLoadedScene;            
        }
        else
        {
            var clientId = NetworkManager.Singleton.LocalClientId;
            Debug.Log("Sending clientId:" + clientId);
            SendClientUsernameToServerServerRpc(clientId, Data.localName);
        }

        GenerateUserStatsForLobby();

        SceneTransitionHandler.sceneTransitionHandler.SetSceneState(SceneTransitionHandler.SceneStates.Lobby);
    }

    private void OnGUI()
    {
        if (lobbyText != null) lobbyText.SetText(UserLobbyStatusText);
    }

    private void GenerateUserStatsForLobby()
    {
        UserLobbyStatusText = string.Empty;
        foreach (var clientLobbyStatus in clientsInLobby)
        {
            var playerName = Data.playerNames.ContainsKey(clientLobbyStatus.Key)
                ? Data.playerNames[clientLobbyStatus.Key]
                : $"Player_{clientLobbyStatus.Key}";

            UserLobbyStatusText += "Player:" + playerName + "            ";
            if (clientLobbyStatus.Value)
            {
                UserLobbyStatusText += "(Ready)\n";
            }
            else
            {
                UserLobbyStatusText += "(Not Ready)\n";
            }
        }
    }

    private void UpdateAndCheckPlayersInLobby()
    {
        allPlayersInLobby = clientsInLobby.Count >= minimumPlayerCount;

        foreach (var clientLobbyStatus in clientsInLobby)
        {
            SendClientReadyStatusUpdatesClientRpc(clientLobbyStatus.Key, clientLobbyStatus.Value);
            if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientLobbyStatus.Key))
            {
                allPlayersInLobby = false;
            }
        }
        CheckForAllPlayersReady();
    }

    private void ClientLoadedScene(ulong clientId)
    {
        if (IsServer)
        {
            if (!clientsInLobby.ContainsKey(clientId))
            {
                clientsInLobby.Add(clientId, false);
                GenerateUserStatsForLobby();
            }

            UpdateAndCheckPlayersInLobby();
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (IsServer)
        {
            if (!clientsInLobby.ContainsKey(clientId))
            {
                clientsInLobby.Add(clientId, false);
            }
            UpdateAndCheckPlayersInLobby();
        }
    }

    [ClientRpc]
    private void SendClientReadyStatusUpdatesClientRpc(ulong clientId, bool isReady)
    {
        Debug.Log("Client Receiving Updates");
        if (!IsServer)
        {
            if (!clientsInLobby.ContainsKey(clientId))
            {
                clientsInLobby.Add(clientId, isReady);
            }
            else
            {
                clientsInLobby[clientId] = isReady;
            }
            GenerateUserStatsForLobby();
        }
    }

    private void CheckForAllPlayersReady()
    {
        if (allPlayersInLobby)
        {
            var allPlayersReady = true;
            foreach (var clientLobbyStatus in clientsInLobby)
            {
                if (!clientLobbyStatus.Value)
                {
                    allPlayersReady = false;
                }
            }
            if (allPlayersReady)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
                SceneTransitionHandler.sceneTransitionHandler.OnClientLoadedScene -= ClientLoadedScene;
                SceneTransitionHandler.sceneTransitionHandler.SwitchScene(inGameSceneName);
            }
        }
    }

    public void PlayerIsReady()
    {
        Debug.Log("Player is Ready");
        clientsInLobby[NetworkManager.Singleton.LocalClientId] = true;
        if (IsServer)
        {
            Debug.Log($"[SERVER] detect {NetworkManager.Singleton.LocalClientId} ready");
            Debug.Log($"Ready value is {clientsInLobby[NetworkManager.Singleton.LocalClientId]}");
            UpdateAndCheckPlayersInLobby();
        }
        else
        {
            OnClientIsReadyServerRpc(NetworkManager.Singleton.LocalClientId);
            Debug.Log($"[CLIENT] detect {NetworkManager.Singleton.LocalClientId} ready");
        }

        GenerateUserStatsForLobby();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnClientIsReadyServerRpc(ulong clientId)
    {
        Debug.Log($"PlayerReady Rpc for {clientId}");
        if (clientsInLobby.ContainsKey(clientId))
        {
            clientsInLobby[clientId] = true;
            UpdateAndCheckPlayersInLobby();
            GenerateUserStatsForLobby();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendClientUsernameToServerServerRpc(ulong clientId, string name)
    {
        if (IsServer)
        {
            Data.AddPlayerName(clientId, name);
            GenerateUserStatsForLobby();
            foreach (var client in Data.playerNames)
            {
                SendPlayerUsernamesToClientsClientRpc(client.Key, Data.playerNames[client.Key]);
            }
        }
    }

    [ClientRpc]
    private void SendPlayerUsernamesToClientsClientRpc(ulong clientId, string name)
    {
        Debug.Log("Client Receiving Updates about names");
        if (!IsServer)
        {
            Data.AddPlayerName(clientId, name);
            GenerateUserStatsForLobby();
        }
    }

    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        SceneTransitionHandler.sceneTransitionHandler.ExitAndLoadStartMenu();
    }
}
