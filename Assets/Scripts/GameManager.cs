using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

public class GameManager : NetworkBehaviour
{
    [Header("Prefab settings")]
    public GameObject redX;
    public GameObject blueO;
    public GameObject[] squares;
    public Transform[] spawnPoints;

    [Header("UI Settings")]
    public TMP_Text currentPlayerText;
    public TMP_Text player1Text;
    public TMP_Text player2Text;
    public TMP_Text player1Score;
    public TMP_Text player2Score;
    public TMP_Text count;

    private bool m_ClientGameOver;
    private bool m_ClientGameStarted;


    public NetworkVariable<bool> hasGameStarted { get; } = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isGameOver { get; } = new NetworkVariable<bool>(false);
    public NetworkVariable<int> counter { get; } = new NetworkVariable<int>(0);
    public NetworkVariable<FixedString32Bytes> playerTurn { get; } = new NetworkVariable<FixedString32Bytes>("0");


    public static GameManager Singleton { get; private set; }

    private void Awake()
    {
        Assert.IsNull(Singleton, $"Multiple instances of {nameof(GameManager)} detected. This should not happen.");
        Singleton = this;

        OnSingletonReady?.Invoke();

        if (IsServer)
        {
            hasGameStarted.Value = false;
            
            //playerTurn.Value = Data.playerNames[0];

            ////Set our time remaining locally
            //m_TimeRemaining = m_DelayedStartTime;

            ////Set for server side
            //m_ReplicatedTimeSent = false;
        }
        else
        {
            //We do a check for the client side value upon instantiating the class (should be zero)
            //Debug.LogFormat("Client side we started with a timer value of {0}", m_TimeRemaining);
        }

        player1Text.SetText(Data.playerNames[0]);
        player2Text.SetText(Data.playerNames[1]);
    }

    internal static event Action OnSingletonReady;

    private void Update()
    {
        //Is the game over?
        if (IsCurrentGameOver()) return;

        //Update game timer (if the game hasn't started)
        //UpdateGameTimer();

        //If we are a connected client, then don't update the enemies (server side only)
        if (!IsServer) return;

        //If we are the server and the game has started, then update the enemies
        //if (HasGameStarted()) UpdateEnemies();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            //m_Enemies.Clear();
            //m_Shields.Clear();
        }

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsServer)
        {
            m_ClientGameOver = false;
            m_ClientGameStarted = false;

            //m_CountdownStarted.OnValueChanged += (oldValue, newValue) =>
            //{
            //    m_ClientStartCountdown = newValue;
            //    Debug.LogFormat("Client side we were notified the start count down state was {0}", newValue);
            //};

            hasGameStarted.OnValueChanged += (oldValue, newValue) =>
            {
                m_ClientGameStarted = newValue;
                //gameTimerText.gameObject.SetActive(!m_ClientGameStarted);
                Debug.LogFormat("Client side we were notified the game started state was {0}", newValue);
            };

            isGameOver.OnValueChanged += (oldValue, newValue) =>
            {
                m_ClientGameOver = newValue;
                Debug.LogFormat("Client side we were notified the game over state was {0}", newValue);
            };
        }

        //Both client and host/server will set the scene state to "ingame" which places the PlayerControl into the SceneTransitionHandler.SceneStates.INGAME
        //and in turn makes the players visible and allows for the players to be controlled.
        SceneTransitionHandler.sceneTransitionHandler.SetSceneState(SceneTransitionHandler.SceneStates.Ingame);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        base.OnNetworkSpawn();
    }

    private void OnClientConnected(ulong clientId)
    {
        //if (m_ReplicatedTimeSent)
        //{
        //    // Send the RPC only to the newly connected client
        //    SetReplicatedTimeRemainingClientRPC(m_TimeRemaining, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong>() { clientId } } });
        //}
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            hasGameStarted.Value = true;
        }
    }

    private bool IsCurrentGameOver()
    {
        if (IsServer)
            return isGameOver.Value;
        return m_ClientGameOver;
    }

    private bool HasGameStarted()
    {
        if (IsServer)
            return hasGameStarted.Value;
        return m_ClientGameStarted;
    }

    private void OnGameStarted()
    {
        //gameTimerText.gameObject.SetActive(false);
        //CreateEnemies();
        //CreateShields();
        //CreateSuperEnemy();
    }

    public void SetActivePlayer(int player)
    {
        currentPlayerText.SetText("Current Player: {0}", player);
    }

    public void AddCount()
    {
        counter.Value++;
        count.SetText(counter.Value.ToString());
    }

    public void SetCount(int c)
    {
        counter.Value = c;
        count.SetText(counter.Value.ToString());
    }

    public void SetCurrentPlayerText(string s)
    {
        //playerTurn.Value = s;
        currentPlayerText.SetText(s);
    }

    public void UpdateCurrentPlayer()
    {
        SetCurrentPlayerText(Data.playerNames[(ulong)int.Parse(playerTurn.Value.ToString())]);
    }

    public void SwapPlayerTurn(ulong clientId)
    {
        Debug.Log($"Swap attempt client:{clientId} playerTurn:{playerTurn.Value.ToString()}");
        if (playerTurn.Value.ToString() != clientId.ToString()) return;

        Debug.Log($"Player {clientId} has completed their turn");
        ulong currentPlayer  = NetworkManager.Singleton.ConnectedClientsIds.Where(id => id != clientId).First();
        playerTurn.Value = currentPlayer.ToString();
        Debug.Log($"Player {currentPlayer} is starting their turn");
        
        SetCurrentPlayerText(Data.playerNames[currentPlayer]);
    }

    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        SceneTransitionHandler.sceneTransitionHandler.ExitAnsLoadStartMenu();
    }
}
