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
    private int count;
    private ulong currentPlayer = 0;

    private void Awake()
    {
        hasGameStarted = false;
    }

    private void Update()
    {
        if (!IsLocalPlayer || !IsOwner) return;

        if (!hasGameStarted) return;

        if (currentPlayer != NetworkManager.Singleton.LocalClientId) return;

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
            activePlayer.OnValueChanged -= OnActivePlayerChanged;
        }

        if (GameManager.Singleton)
        {
            GameManager.Singleton.isGameOver.OnValueChanged -= OnGameStartedChanged;
            GameManager.Singleton.hasGameStarted.OnValueChanged -= OnGameStartedChanged;
            GameManager.Singleton.counter.OnValueChanged -= OnCountChanged;
            GameManager.Singleton.playerTurn.OnValueChanged -= OnPlayerTurnChanged;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //GameManager.Singleton.UpdateCurrentPlayer();

        activePlayer.OnValueChanged += OnActivePlayerChanged;

        if (IsServer) ownerRPCParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } } };

        if (!GameManager.Singleton)
            GameManager.OnSingletonReady += SubscribeToDelegatesAndUpdateValues;
        else
            SubscribeToDelegatesAndUpdateValues();
    }

    private void SubscribeToDelegatesAndUpdateValues()
    {
        GameManager.Singleton.hasGameStarted.OnValueChanged += OnGameStartedChanged;
        GameManager.Singleton.isGameOver.OnValueChanged += OnGameStartedChanged;
        GameManager.Singleton.counter.OnValueChanged += OnCountChanged;
        GameManager.Singleton.playerTurn.OnValueChanged += OnPlayerTurnChanged;

        if (IsClient && IsOwner)
        {
            GameManager.Singleton.SetActivePlayer(activePlayer.Value);
        }

        hasGameStarted = GameManager.Singleton.hasGameStarted.Value;
    }

    private bool CheckForClicks()
    {
        for (int i = 0; i < GameManager.Singleton.squares.Length; i++)
        {

            Tile t = GameManager.Singleton.squares[i].GetComponent<Tile>();
            if (t.clicked && t.spawned)
            {
                t.spawned = true;
                GameObject shape = new GameObject();
                if (NetworkManager.Singleton.LocalClientId == 0)
                {
                    shape = Instantiate(GameManager.Singleton.redX,GameManager.Singleton.spawnPoints[i]);
                }
                else
                {
                    shape = Instantiate(GameManager.Singleton.blueO, GameManager.Singleton.spawnPoints[i]);
                }
                shape.GetComponent<NetworkObject>().Spawn();
                return true;
            }
        }
        return false;
    }

    private void OnActivePlayerChanged(int previous, int current)
    {
        if (!IsOwner) return;
        Debug.LogFormat("Lives {0} ", current);

        if (GameManager.Singleton != null) GameManager.Singleton.SetActivePlayer(1);
    }

    private void OnGameStartedChanged(bool previousValue, bool newValue)
    {
        hasGameStarted = newValue;
    }

    private void OnCountChanged(int previous, int current)
    {
        if (!IsOwner) return;
        GameManager.Singleton.SetCount(current);
    }

    private void OnPlayerTurnChanged(FixedString32Bytes previous, FixedString32Bytes current)
    {
        if (!IsOwner) return;
        Debug.Log("Player Turn Change");
        string playerName = Data.playerNames[(ulong)int.Parse(current.ToString())];
        GameManager.Singleton.SetCurrentPlayerText(playerName);
        currentPlayer = (ulong)int.Parse(current.ToString());
    }

    [ServerRpc]
    private void ShootServerRPC()
    {
        GameManager.Singleton.AddCount();
        //if (!m_IsAlive)
        //    return;

        //if (m_MyBullet == null)
        //{
        //    m_MyBullet = Instantiate(bulletPrefab, transform.position + Vector3.up, Quaternion.identity);
        //    m_MyBullet.GetComponent<PlayerBullet>().owner = this;
        //    m_MyBullet.GetComponent<NetworkObject>().Spawn();
        //}
    }

    [ServerRpc]
    private void ChangePlayerTurnServerRpc(ulong clientId)
    {
        GameManager.Singleton.SwapPlayerTurn(clientId);
    }
}
