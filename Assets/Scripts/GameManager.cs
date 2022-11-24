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
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    [Header("Prefab settings")]
    public GameObject redX;
    public GameObject blueO;
    public GameObject[] squares;
    public Transform[] spawnPoints;

    [Header("UI Settings")]
    public TMP_Text currentPlayerText;
    public Image currentPlayerImage;
    public TMP_Text player1Text;
    public TMP_Text player2Text;
    public TMP_Text player1Score;
    public TMP_Text player2Score;
    public TMP_Text count;
    public TMP_Text winnerText;
    public GameObject player1Wins;
    public GameObject player2Wins;
    public Image player1Check;
    public Image player2Check;

    //private bool player1Retry;
    //private bool player2Retry;

    private bool m_ClientGameOver;
    private bool m_ClientGameStarted;

    public List<GameObject> pieces;


    public NetworkVariable<bool> hasGameStarted { get; } = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isGameOver { get; } = new NetworkVariable<bool>(false);
    public NetworkVariable<int> score1 { get; } = new NetworkVariable<int>(0);
    public NetworkVariable<int> score2 { get; } = new NetworkVariable<int>(0);
    public NetworkVariable<FixedString32Bytes> playerTurn { get; } = new NetworkVariable<FixedString32Bytes>("0");
    public NetworkVariable<FixedString32Bytes> winner { get; } = new NetworkVariable<FixedString32Bytes>("0");
    public NetworkVariable<bool> player1Retry { get; } = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> player2Retry { get; } = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> newGame { get; } = new NetworkVariable<bool>(false);




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
        Debug.Log("GameManager Awake");
        player1Check.enabled = false;
        player2Check.enabled = false;
        player1Wins.SetActive(false);
        player2Wins.SetActive(false);
        pieces = new List<GameObject>();
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
        currentPlayerText.SetText($"{s}'s\nTurn");
    }

    public void UpdateCurrentPlayer()
    {
        SetCurrentPlayerText(Data.playerNames[(ulong)int.Parse(playerTurn.Value.ToString())]);
    }

    public void SwapPlayerTurn(ulong clientId)
    {
        if (playerTurn.Value.ToString() != clientId.ToString()) return;

        //Check for Victory before switching
        CheckVictory();
        if (isGameOver.Value) return;

        ulong currentPlayer  = NetworkManager.Singleton.ConnectedClientsIds.Where(id => id != clientId).First();
        playerTurn.Value = currentPlayer.ToString();
        
        SetCurrentPlayerText(Data.playerNames[currentPlayer]);
        if (currentPlayer == 0)
        {
            currentPlayerImage.color = Color.red;
        }
        else
        {
            currentPlayerImage.color = Color.blue;
        }
    }

    private void CheckVictory()
    {
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
        ulong winningPlayer = 0;
        
        isGameOver.Value = true;
        
        if (playerTurn.Value == "0")
        {
            UpdatePlayer1Score(score1.Value++);
            winner.Value = Data.playerNames[0];
        }
        else
        {
            UpdatePlayer2Score(score2.Value++);
            winner.Value = Data.playerNames[1];
            winningPlayer = 1;
        }

        SetVictoryText(winningPlayer);
        
    }

    public void SetVictoryText(ulong winner)
    {
        if (winner == 0)
        {
            player1Wins.SetActive(true);
            player1Wins.transform.GetChild(1).GetComponent<TMP_Text>().SetText($"{Data.playerNames[winner]}\nWins!");
            player1Wins.GetComponentInChildren<Button>().onClick.AddListener(RetryListener);
        }
        else
        {
            player2Wins.SetActive(true);
            player2Wins.transform.GetChild(1).GetComponent<TMP_Text>().SetText($"{Data.playerNames[winner]}\nWins!");
            player2Wins.GetComponentInChildren<Button>().onClick.AddListener(RetryListener);
        }
    }

    //private void RetryListener()
    //{
    //    if (NetworkManager.Singleton.LocalClientId == 0)
    //    {
    //        player1Check.enabled = true;
    //        player1Retry.Value = true;
    //    }
    //    else
    //    {
    //        player2Check.enabled = true;
    //        player2Retry.Value = true;
    //    }
    //    CheckRematch();
    //}

    
    private void RetryListener()
    {
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            SetPlayer1CheckServerRpc();
        }
        else
        {
            SetPlayer2CheckServerRpc();
        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayer1CheckServerRpc()
    {
        player1Check.enabled = true;
        player1Retry.Value = true;
        CheckRematch();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayer2CheckServerRpc()
    {
        player2Check.enabled = true;
        player2Retry.Value = true;
        CheckRematch();
    }

    private void CheckRematch()
    {
        if (player1Retry.Value && player2Retry.Value)
        {
            newGame.Value = true;
        }
    }

    public void ResetMatch()
    {
        foreach(var s in squares)
        {
            s.GetComponent<Tile>().Reset();
        }
        foreach(var p in pieces)
        {
            Destroy(p);
        }
        pieces.Clear();

        player1Check.enabled = false;
        player2Check.enabled = false;

        ulong currentPlayer = 0;
        playerTurn.Value = currentPlayer.ToString();

        SetCurrentPlayerText(Data.playerNames[currentPlayer]);
        currentPlayerImage.color = Color.red;
    }

    public void UpdatePlayer1Retry(bool b)
    {
        player1Retry.Value = b;
        player1Check.enabled = b;
    }

    public void UpdatePlayer2Retry(bool b)
    {
        player2Retry.Value = b;
        player2Check.enabled = b;
    }

    public void UpdatePlayer1Score(int s)
    {
        score1.Value = s;
        player1Score.SetText("Score - " + s);
    }

    public void UpdatePlayer2Score(int s)
    {
        score2.Value = s;
        player2Score.SetText("Score - " + s);
    }

    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        SceneTransitionHandler.sceneTransitionHandler.ExitAnsLoadStartMenu();
    }
}
