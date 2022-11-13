using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Header("Player Settings")]
    private int localActivePlayer = 1;
    private NetworkVariable<int> activePlayer = new NetworkVariable<int>(1);
    private ClientRpcParams ownerRPCParams;
    private bool hasGameStarted;
    private bool isGameOver;
    private int count;
    private ulong currentPlayer = 0;

    private void Awake()
    {
        hasGameStarted = false;
    }

    private void Update()
    {
        Debug.Log("Updating");
        if (!IsLocalPlayer || !IsOwner) return;

        Debug.Log($"Is player gameStarted:{hasGameStarted}");

        if (!hasGameStarted) return;

        Debug.Log($"Game Started currentPlayer:{currentPlayer} clientId:{NetworkManager.Singleton.LocalClientId}");

        if (currentPlayer != NetworkManager.Singleton.LocalClientId) return;

        if (isGameOver) return;

        if (CheckForClicks())
        {
            ChangePlayerTurnServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {

            ChangePlayerTurnServerRpc(NetworkManager.Singleton.LocalClientId);
            //ShootServerRPC();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsClient)
        {

        }

        if (GameManager.Singleton)
        {
            GameManager.Singleton.isGameOver.OnValueChanged -= IsGameOverChanged;
            GameManager.Singleton.hasGameStarted.OnValueChanged -= OnGameStartedChanged;
            GameManager.Singleton.playerTurn.OnValueChanged -= OnPlayerTurnChanged;
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

        if (IsClient && IsOwner)
        {
            
        }

        hasGameStarted = GameManager.Singleton.hasGameStarted.Value;
    }

    private bool CheckForClicks()
    {
        Debug.Log($"Check Clicks size:{GameManager.Singleton.squares.Length}");
        for (int i = 0; i < GameManager.Singleton.squares.Length; i++)
        {

            Tile t = GameManager.Singleton.squares[i].GetComponent<Tile>();
            if (t.clicked && !t.spawned)
            {
                Debug.Log($"Spawn at tile:{i}");
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
            string winner = Data.playerNames[currentPlayer];
            GameManager.Singleton.SetVictoryText(winner);
        }
    }

    private void OnPlayerTurnChanged(FixedString32Bytes previous, FixedString32Bytes current)
    {
        if (!IsOwner) return;
        Debug.Log("Player Turn Change");
        string playerName = Data.playerNames[(ulong)int.Parse(current.ToString())];
        GameManager.Singleton.SetCurrentPlayerText(playerName);
        currentPlayer = (ulong)int.Parse(current.ToString());
        ClearAllClicks();
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
    }

    [ServerRpc]
    private void SpawnBlueOServerRpc(int pos)
    {
        GameObject shape = Instantiate(GameManager.Singleton.blueO, GameManager.Singleton.spawnPoints[pos].position, Quaternion.Euler(0, 0, 0));
        shape.GetComponent<Rigidbody>().isKinematic = false;
        shape.GetComponent<NetworkObject>().Spawn();
        GameManager.Singleton.squares[pos].GetComponent<Tile>().player = (int)currentPlayer;
        MarkSquareClientRpc(pos);
    }

    [ClientRpc]
    private void MarkSquareClientRpc(int pos)
    {
        GameManager.Singleton.squares[pos].GetComponent<Tile>().spawned = true;
        GameManager.Singleton.squares[pos].GetComponent<Tile>().player = (int)currentPlayer;
        Debug.Log($"Setting pos:{pos} to spawned");
    }


}
