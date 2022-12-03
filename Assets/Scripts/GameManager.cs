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
using UnityEngine.SceneManagement;

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
    public GameObject QuitAlert;

    public Color PLAYER1COLOR;
    public Color PLAYER2COLOR;

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
    public NetworkVariable<int> winner { get; } = new NetworkVariable<int>(0);
    
    public NetworkVariable<bool> newGame { get; } = new NetworkVariable<bool>(false);

    public int winningPlayer = 0;
    public int p1score = 0;
    public int p2score = 0;

    public bool p1Rematch;
    public bool p2Rematch;
    public bool opponentQuit;

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
        QuitAlert.SetActive(false);
        pieces = new List<GameObject>();
    }

    internal static event Action OnSingletonReady;

    private void Update()
    {
        if (opponentQuit) return;

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

    private void DetectHostDisconnect()
    {
        Debug.Log("Detect Host Disconnect");
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsServer)
        {
            NetworkManager.Singleton.OnTransportFailure += DetectHostDisconnect;
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
        Debug.Log("ATTEMPT SWAP PLAYER");
        if (!IsServer) return;
        if (isGameOver.Value) return;

        if (playerTurn.Value.ToString() != clientId.ToString()) return;

        //Check for Victory before switching
        CheckVictory();
        if (isGameOver.Value) return;

        ulong currentPlayer  = NetworkManager.Singleton.ConnectedClientsIds.Where(id => id != clientId).First();
        playerTurn.Value = currentPlayer.ToString();
        
        SetCurrentPlayerText(Data.playerNames[currentPlayer]);
        UpdateCurrentPlayerColor(currentPlayer);
        Debug.Log($"Setting current player to {currentPlayer}");
    }

    public void UpdateCurrentPlayerColor(ulong currentPlayer)
    {
        if (currentPlayer == 0)
        {
            currentPlayerImage.color = PLAYER1COLOR;
        }
        else
        {
            currentPlayerImage.color = PLAYER2COLOR;
        }
    }

    private void CheckVictory()
    {
        if (!IsServer) return;

        int TL = squares[0].GetComponent<Tile>().player;
        int TM = squares[1].GetComponent<Tile>().player;
        int TR = squares[2].GetComponent<Tile>().player;

        int ML = squares[3].GetComponent<Tile>().player;
        int MM = squares[4].GetComponent<Tile>().player;
        int MR = squares[5].GetComponent<Tile>().player;

        int BL = squares[6].GetComponent<Tile>().player;
        int BM = squares[7].GetComponent<Tile>().player;
        int BR = squares[8].GetComponent<Tile>().player;

        if (TL == TM && TM == TR && TL != -1)
        {
            SetVictory();
            return;
        }

        if (ML == MM && MM == MR && MR != -1)
        {
            SetVictory();
            return;
        }

        if (BL == BM && BM == BR && BR != -1)
        {
            SetVictory();
            return;
        }

        if (TL == ML && ML == BL && BL != -1)
        {
            SetVictory();
            return;
        }
        if (TM == MM && MM == BM && BM != -1)
        {
            SetVictory();
            return;
        }
        if (TR == MR && MR == BR && BR != -1)
        {
            SetVictory();
            return;
        }

        if (TL == MM && MM == BR && BR != -1)
        {
            SetVictory();
            return;
        }
        if (TR == MM && MM == BL && BL != -1)
        {
            SetVictory();
            return;
        }

        Debug.Log($"{TL} , {TM} , {TR}\n{ML} , {MM} , {MR}\n{BL} , {BM} , {BR}");
    }

    public void SetVictory()
    {
        Debug.Log("Set Victory for player:" + playerTurn.Value);
        
        if (playerTurn.Value.ToString() == "0")
        {
            Debug.Log("Player 0 wins");
            UpdatePlayer1ScoreClientRpc(p1score + 1);
            winner.Value = 0;
            SetWinnerClientRpc(0);
        }
        else if (playerTurn.Value.ToString() == "1")
        {
            Debug.Log("Player 1 wins");
            UpdatePlayer2ScoreClientRpc(p2score + 1);
            winner.Value = 1;
            SetWinnerClientRpc(1);
        }
        else
        {
            Debug.LogError("INCORRECT VICTORY PLAYER");
        }

        SetVictoryText();

        isGameOver.Value = true;

    }

    public void SetVictoryText()
    {
        Debug.Log("Setting Victory text while GameOver = " + isGameOver.Value);
        if (winningPlayer == 0)
        {
            Debug.Log("Set P1 Victory Banner");
            player1Wins.SetActive(true);
            player1Wins.transform.GetChild(1).GetComponent<TMP_Text>().SetText($"{Data.playerNames[(ulong)winningPlayer]}\nWins!");
            player1Wins.GetComponentInChildren<Button>().onClick.AddListener(RetryListener);
        }
        else if (winningPlayer  == 1)
        {
            Debug.Log("Set P2 Victory Banner");
            player2Wins.SetActive(true);
            player2Wins.transform.GetChild(1).GetComponent<TMP_Text>().SetText($"{Data.playerNames[(ulong)winningPlayer]}\nWins!");
            player2Wins.GetComponentInChildren<Button>().onClick.AddListener(RetryListener);
        }
    }
    
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
        p1Rematch = true;
        SetRematchClientRpc(0, true);
        CheckRematch();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayer2CheckServerRpc()
    {
        p2Rematch = true;
        SetRematchClientRpc(1, true);
        CheckRematch();
    }

    private void CheckRematch()
    {
        if (p1Rematch && p2Rematch)
        {
            //newGame.Value = true;
            
            ResetMatchClientRpc();
        }
    }

    [ClientRpc]
    public void ResetMatchClientRpc()
    {
        player1Wins.SetActive(false);
        player2Wins.SetActive(false);

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

        SetCurrentPlayerText(Data.playerNames[currentPlayer]);
        currentPlayerImage.color = PLAYER1COLOR;

        Debug.Log("Reset Client RPC");

        ResetServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetServerRpc()
    {
        isGameOver.Value = false;
        newGame.Value = false;
        p1Rematch = false;
        p2Rematch = false;
        playerTurn.Value = "0";
        hasGameStarted.Value = true;
        SetRematchClientRpc(0, false);
        SetRematchClientRpc(1, false);
    }

    public void UpdatePlayer1Retry(bool b)
    {
        //player1Retry.Value = b;
        //player1Check.enabled = b;
    }

    public void UpdatePlayer2Retry(bool b)
    {
        //player2Retry.Value = b;
        //player2Check.enabled = b;
    }

    [ClientRpc]
    public void UpdatePlayer1ScoreClientRpc(int s)
    {
        p1score = s;
        player1Score.SetText("Score - " + s);
    }

    [ClientRpc]
    public void UpdatePlayer2ScoreClientRpc(int s)
    {
        p2score = s;
        player2Score.SetText("Score - " + s);
    }

    public void ExitGame()
    {
        if (IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            SceneTransitionHandler.sceneTransitionHandler.ExitAndLoadStartMenu();
        }
        else
        {
            QuitAlertServerRpc();
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }
        
    }

    [ClientRpc]
    private void SetWinnerClientRpc(int i)
    {
        winningPlayer = i;
    }

    [ClientRpc]
    private void SetRematchClientRpc(int player, bool status)
    {
        if (player == 0)
        {
            Debug.Log("Set Player 1 Checkmark " + status + " " + Time.realtimeSinceStartup);
            player1Check.enabled = status;
        }
        else if (player == 1)
        {
            Debug.Log("Set Player 2 Checkmark " + status + " " + Time.realtimeSinceStartup);
            player2Check.enabled = status;
        }
    }

    [ClientRpc]
    public void QuitAlertClientRpc()
    {
        QuitAlert.SetActive(true);
        opponentQuit = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void QuitAlertServerRpc()
    {
        QuitAlert.SetActive(true);
        opponentQuit = true;
    }
}
