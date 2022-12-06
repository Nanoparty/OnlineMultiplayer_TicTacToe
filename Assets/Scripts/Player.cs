using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour
{
    [Header("Player Settings")]
    private ClientRpcParams ownerRPCParams;
    private bool hasGameStarted;
    private bool isGameOver;
    private ulong currentPlayer = 0;
    private int winner = 0;

    private void Awake()
    {
        hasGameStarted = false;
    }

    private void Update()
    {
        if (GameManager.Singleton && GameManager.Singleton.opponentQuit) return;

        if (!IsLocalPlayer || !IsOwner) return;


        if (!hasGameStarted) return;


        if (currentPlayer != NetworkManager.Singleton.LocalClientId) return;

        if (isGameOver) return;

        if (CheckForClicks())
        {
            ChangePlayerTurnServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangePlayerTurnServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            Debug.Log("Host detects other client disconnect");
            GameManager.Singleton?.QuitAlertClientRpc();

            
        }

        if (!IsHost)
        {
            Debug.Log("Client Detects Disconnect");
            NetworkManager.Shutdown();
            SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }
        

        if (GameManager.Singleton)
        {
            GameManager.Singleton.isGameOver.OnValueChanged -= IsGameOverChanged;
            GameManager.Singleton.hasGameStarted.OnValueChanged -= OnGameStartedChanged;
            GameManager.Singleton.playerTurn.OnValueChanged -= OnPlayerTurnChanged;
            GameManager.Singleton.score1.OnValueChanged -= OnScore1Changed;
            GameManager.Singleton.score2.OnValueChanged -= OnScore2Changed;

            if (!IsHost)
            {
                Debug.Log("Client detects Host disconnect");
                GameManager.Singleton.QuitAlert.SetActive(true);
                GameManager.Singleton.opponentQuit = true;
            }
        }

        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer) ownerRPCParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } } };

        if (!GameManager.Singleton)
            GameManager.OnSingletonReady += SubscribeToDelegatesAndUpdateValues;
        else
            SubscribeToDelegatesAndUpdateValues();
    }

    private void SubscribeToDelegatesAndUpdateValues()
    {
        GameManager.Singleton.hasGameStarted.OnValueChanged += OnGameStartedChanged;
        GameManager.Singleton.isGameOver.OnValueChanged += IsGameOverChanged;
        GameManager.Singleton.playerTurn.OnValueChanged += OnPlayerTurnChanged;
        GameManager.Singleton.score1.OnValueChanged += OnScore1Changed;
        GameManager.Singleton.score2.OnValueChanged += OnScore2Changed;
        
        GameManager.Singleton.newGame.OnValueChanged += OnNewGameChanged;
        GameManager.Singleton.winner.OnValueChanged += OnWinnerChanged;

        if (IsClient && IsOwner)
        {
            
        }

        hasGameStarted = GameManager.Singleton.hasGameStarted.Value;
    }

    private bool CheckForClicks()
    {
        for (int i = 0; i < GameManager.Singleton.squares.Length; i++)
        {

            Tile t = GameManager.Singleton.squares[i].GetComponent<Tile>();
            if (t.clicked && !t.spawned)
            {
                t.spawned = true;
                if (NetworkManager.Singleton.LocalClientId == 0)
                {
                    SpawnRedXServerRpc(i);
                }
                else
                {
                    SpawnBlueOServerRpc(i);
                }
                return true;
            }
        }
        return false;
    }

    

    private void OnGameStartedChanged(bool previousValue, bool newValue)
    {
        hasGameStarted = newValue;
        if (newValue)
        {
            GameManager.Singleton.SetCurrentPlayerText(Data.playerNames[currentPlayer]);
        }
    }

    private void IsGameOverChanged(bool previousValue, bool newValue)
    {
        isGameOver = newValue;
        if (newValue)
        {
            GameManager.Singleton.SetVictoryText();
        }
    }

    private void OnWinnerChanged(int previous, int current)
    {
        GameManager.Singleton.winner.Value = current;
    }

    private void OnPlayerTurnChanged(FixedString32Bytes previous, FixedString32Bytes current)
    {
        if (!IsOwner) return;

        string playerName = Data.playerNames[(ulong)int.Parse(current.ToString())];
        GameManager.Singleton.SetCurrentPlayerText(playerName);
        GameManager.Singleton.UpdateCurrentPlayerColor((ulong)int.Parse(current.ToString()));
        currentPlayer = (ulong)int.Parse(current.ToString());
        ClearAllClicks();
    }

    private void OnScore1Changed(int previous, int current)
    {
        //GameManager.Singleton.UpdatePlayer1Score(current);
    }

    private void OnScore2Changed(int previous, int current)
    {
        //GameManager.Singleton.UpdatePlayer2Score(current);
    }

    private void OnPlayer1Retry(bool previous, bool current)
    {
        GameManager.Singleton.UpdatePlayer1Retry(current);
    }

    private void OnPlayer2Retry(bool previous, bool current)
    {
        GameManager.Singleton.UpdatePlayer2Retry(current);
    }

    private void OnNewGameChanged(bool previous, bool current)
    {
        //GameManager.Singleton.ResetMatch();
    }

    private void ClearAllClicks()
    {
        foreach(GameObject s in GameManager.Singleton.squares)
        {
            Tile t = s.GetComponent<Tile>();
            t.clicked = false;
        }
    }

    [ServerRpc]
    private void ChangePlayerTurnServerRpc(ulong clientId)
    {
        GameManager.Singleton.SwapPlayerTurn(clientId);
    }

    [ServerRpc]
    private void SpawnRedXServerRpc(int pos)
    {
        GameObject shape = Instantiate(GameManager.Singleton.redX, GameManager.Singleton.spawnPoints[pos].position, Quaternion.Euler(0f, 45f, 0f));
        shape.GetComponent<Rigidbody>().isKinematic = false;
        shape.GetComponent<NetworkObject>().Spawn();
        GameManager.Singleton.squares[pos].GetComponent<Tile>().player = (int)currentPlayer;
        MarkSquareClientRpc(pos);
        GameManager.Singleton.pieces.Add(shape);
    }

    [ServerRpc]
    private void SpawnBlueOServerRpc(int pos)
    {
        GameObject shape = Instantiate(GameManager.Singleton.blueO, GameManager.Singleton.spawnPoints[pos].position, Quaternion.Euler(0, 0, 0));
        shape.GetComponent<Rigidbody>().isKinematic = false;
        shape.GetComponent<NetworkObject>().Spawn();
        GameManager.Singleton.squares[pos].GetComponent<Tile>().player = (int)currentPlayer;
        MarkSquareClientRpc(pos);
        GameManager.Singleton.pieces.Add(shape);
    }

    [ClientRpc]
    private void MarkSquareClientRpc(int pos)
    {
        GameManager.Singleton.squares[pos].GetComponent<Tile>().spawned = true;
        GameManager.Singleton.squares[pos].GetComponent<Tile>().player = int.Parse(GameManager.Singleton.playerTurn.Value.ToString());
    }

    [ClientRpc]
    public void SetWinnerClientRPC(int i)
    {
        GameManager.Singleton.winner.Value = i;
    }
}
