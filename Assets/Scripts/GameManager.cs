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
    public TMP_Text winnerText;

    private bool m_ClientGameOver;
    private bool m_ClientGameStarted;


    public NetworkVariable<bool> hasGameStarted { get; } = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isGameOver { get; } = new NetworkVariable<bool>(false);
    public NetworkVariable<int> counter { get; } = new NetworkVariable<int>(0);
    public NetworkVariable<FixedString32Bytes> playerTurn { get; } = new NetworkVariable<FixedString32Bytes>("0");
    public NetworkVariable<FixedString32Bytes> winner { get; } = new NetworkVariable<FixedString32Bytes>("0");


    public static GameManager Singleton { get; private set; }

    private void Awake()
    {
        Assert.IsNull(Singleton, $"Multiple instances of {nameof(GameManager)} detected. This should not happen.");
        Singleton = this;

        OnSingletonReady?.Invoke();

        if (IsServer)
        {
            hasGameStarted.Value = false;
            isGameOver.Value = false;
        }
        else
        {

        }

        player1Text.SetText(Data.playerNames[0]);
        player2Text.SetText(Data.playerNames[1]);
        winnerText.gameObject.SetActive(false);
    }

    internal static event Action OnSingletonReady;

    private void Update()
    {
        if (IsCurrentGameOver()) return;

        if (!hasGameStarted.Value) hasGameStarted.Value = true;

        if (!IsServer) return;

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {

        }

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsServer)
        {
            m_ClientGameOver = false;
            m_ClientGameStarted = false;

            hasGameStarted.OnValueChanged += (oldValue, newValue) =>
            {
                m_ClientGameStarted = newValue;
                Debug.LogFormat("Client side we were notified the game started state was {0}", newValue);
            };

            isGameOver.OnValueChanged += (oldValue, newValue) =>
            {
                m_ClientGameOver = newValue;
                Debug.LogFormat("Client side we were notified the game over state was {0}", newValue);
            };
        }

        SceneTransitionHandler.sceneTransitionHandler.SetSceneState(SceneTransitionHandler.SceneStates.Ingame);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        base.OnNetworkSpawn();
    }

    private void OnClientConnected(ulong clientId)
    {
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
        
    }

    public void SetCurrentPlayerText(string s)
    {
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

        //Check for Victory before switching
        CheckVictory();
        if (isGameOver.Value) return;

        Debug.Log($"Player {clientId} has completed their turn");
        ulong currentPlayer  = NetworkManager.Singleton.ConnectedClientsIds.Where(id => id != clientId).First();
        playerTurn.Value = currentPlayer.ToString();
        Debug.Log($"Player {currentPlayer} is starting their turn");
        
        SetCurrentPlayerText(Data.playerNames[currentPlayer]);
    }

    private void CheckVictory()
    {
        Debug.Log("Check Victory");
        int TL = squares[0].GetComponent<Tile>().player;
        int TM = squares[1].GetComponent<Tile>().player;
        int TR = squares[2].GetComponent<Tile>().player;

        int ML = squares[3].GetComponent<Tile>().player;
        int MM = squares[4].GetComponent<Tile>().player;
        int MR = squares[5].GetComponent<Tile>().player;

        int BL = squares[6].GetComponent<Tile>().player;
        int BM = squares[7].GetComponent<Tile>().player;
        int BR = squares[8].GetComponent<Tile>().player;

        if (TL == TM && TM == TR && TL != -1) SetVictory();
        if (ML == MM && MM == MR && MR != -1) SetVictory();
        if (BL == BM && BM == BR && BR != -1) SetVictory();

        if (TL == ML && ML == BL && BL != -1) SetVictory();
        if (TM == MM && MM == BM && BM != -1) SetVictory();
        if (TR == MR && MR == BR && BR != -1) SetVictory();

        if (TL == MM && MM == BR && BR != -1) SetVictory();
        if (TR == MM && MM == BL && BL != -1) SetVictory();

        Debug.Log($"{TL} - {TM} - {TR} - {ML} - {MM} - {MR} - {BL} - {BM} - {BR}");
    }

    public void SetVictory()
    {
        Debug.Log("Set Victory");
        
        isGameOver.Value = true;
        
        if (playerTurn.Value == "0")
        {
            winner.Value = Data.playerNames[0];
        }
        else
        {
            winner.Value = Data.playerNames[1];
        }

        SetVictoryText(winner.Value.ToString());
    }

    public void SetVictoryText(string winner)
    {
        winnerText.text = $"{winner} won!";
        winnerText.gameObject.SetActive(true);
    }

    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        SceneTransitionHandler.sceneTransitionHandler.ExitAnsLoadStartMenu();
    }
}
