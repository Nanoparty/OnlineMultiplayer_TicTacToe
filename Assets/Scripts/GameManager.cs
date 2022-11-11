using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameObject redX;
    public GameObject blueO;

    public Transform[] spawnPoints;
    public GameObject[] squares;

    public TMP_Text currentPlayerText;
    public TMP_Text player1Text;
    public TMP_Text player2Text;
    public TMP_Text player1Score;
    public TMP_Text player2Score;

    private void Start()
    {
        
    }

    private void UpdateText()
    {
        //player1Text.text = player1Name.Value.ToString();
        //player2Text.text = player2Name.Value.ToString();
        //currentPlayerText.text = currentPlayer.Value == 1 ? player1Name.Value.ToString() : player2Name.Value.ToString();

    }

    public override void OnNetworkSpawn()
    {
        //currentPlayer.OnValueChanged += (int previousValue, int newValue) =>
        //{
        //    Debug.Log("New Current Player:" + newValue);
        //    UpdateText();
        //};
        //player1Name.OnValueChanged += (FixedString128Bytes previousValue, FixedString128Bytes newValue) => {
        //    Debug.Log("Player 1 name:" + newValue);
        //    UpdateText();
        //};
        //player2Name.OnValueChanged += (FixedString128Bytes previousValue, FixedString128Bytes newValue) =>
        //{
        //    Debug.Log("Player 2 name:" + newValue);
        //    UpdateText();
        //};
    }

    private void Update()
    {
        ////Debug.Log("Host:"+NetworkManager.Singleton.IsHost);
        ////Debug.Log("Client:" + NetworkManager.Singleton.IsClient);
        ////Debug.Log("ID:" + NetworkManager.Singleton.LocalClientId);
        //if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClientsList.Count == 1)
        //{
        //    Debug.Log("Waiting");
        //    return;
        //}
        //else
        //{
        //    waitingForPlayer.SetActive(false);
        //    Debug.Log("Not Waiting");
        //}

        //if (currentPlayer.Value == 1 && NetworkManager.Singleton.LocalClientId == 1) return;
        //if (currentPlayer.Value == 2 && NetworkManager.Singleton.LocalClientId == 0) return;

        //if (currentPlayer.Value == 1)
        //{
        //    for (int i = 0; i < squares.Length; i++)
        //    {
        //        Tile t = squares[i].GetComponent<Tile>();
        //        if (t.clicked && !t.spawned)
        //        {
        //            //t.spawned = true;
        //            //GameObject o = Instantiate(blueO, spawnPoints[i].transform.position, Quaternion.identity);
        //            //o.GetComponent<NetworkObject>().Spawn(true);
        //            currentPlayer.Value = 2;
        //            UpdateText();
        //        }
        //    }
        //}
        //else if (currentPlayer.Value == 2)
        //{
        //    for (int i = 0; i < squares.Length; i++)
        //    {
        //        Tile t = squares[i].GetComponent<Tile>();
        //        if (t.clicked && !t.spawned)
        //        {
        //            //t.spawned = true;
        //            //GameObject o = Instantiate(redX, spawnPoints[i].transform.position, Quaternion.Euler(0f, 45f, 0f));
        //            //o.GetComponent<NetworkObject>().Spawn(true);
        //            currentPlayer.Value = 1;
        //            UpdateText();
        //        }
        //    }
        //}
        //UpdateText();
    }

    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        SceneTransitionHandler.sceneTransitionHandler.ExitAnsLoadStartMenu();
    }
}
