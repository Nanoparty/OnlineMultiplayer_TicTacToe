using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalGameManager : MonoBehaviour
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

    bool playerTurn;
    bool victory;
    string winner;

    private void Start()
    {
        player1Text.SetText(Data.localName ?? "Player 1");
        player2Text.SetText("Player 2");

        winnerText.gameObject.SetActive(false);

        playerTurn = true;
    }

    private void Update()
    {
        if (victory) return;

        if (playerTurn)
        {
            PlayerUpdate();
        }
        else
        {
            AIUpdate();
        }

        CheckVictory();
        
    }

    private void PlayerUpdate()
    {
        for (int i = 0; i < squares.Length; i++)
        {
            Tile t = squares[i].GetComponent<Tile>();
            if (t.clicked && !t.spawned)
            {
                t.spawned = true;
                t.player = 1;
                GameObject o = Instantiate(redX, spawnPoints[i].position, Quaternion.Euler(0f, 45f, 0f));
                o.GetComponent<Rigidbody>().isKinematic = false;
                playerTurn = false;
                return;
            }
        }
    }

    private void AIUpdate()
    {
        List<int> options = new List<int> { 0,1,2,3,4,5,6,7,8 };
        for(int i = 0; i < squares.Length; i++)
        {
            if (squares[i].GetComponent<Tile>().spawned)
            {
                options.Remove(i);
                Debug.Log($"Removing {i}");
            }
        }
        if (options.Count == 0) return;

        int pick = options[Random.Range(0, options.Count)];
        Debug.Log($"Picking {pick}");
        squares[pick].GetComponent<Tile>().spawned = true;
        squares[pick].GetComponent<Tile>().player = 2;
        GameObject o = Instantiate(blueO, spawnPoints[pick].position, Quaternion.Euler(0f, 0f, 0f));
        o.GetComponent<Rigidbody>().isKinematic = false;
        playerTurn = true;
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

        if (TL == TM && TM == TR && TL != 0) SetVictory();
        if (ML == MM && MM == MR && MR != 0) SetVictory();
        if (BL == BM && BM == BR && BR != 0) SetVictory();

        if (TL == ML && ML == BL && BL != 0) SetVictory();
        if (TM == MM && MM == BM && BM != 0) SetVictory();
        if (TR == MR && MR == BR && BR != 0) SetVictory();

        if (TL == MM && MM == BR && BR != 0) SetVictory();
        if (TR == MM && MM == BL && BL != 0) SetVictory();
    }

    private void SetVictory()
    {
        victory = true;
        if (playerTurn)
        {
            winner = "Player 2";
        }
        else
        {
            winner = "Player 1";
        }

        winnerText.text = $"{winner} won!";
        winnerText.gameObject.SetActive(true);
    }

    public void Quit()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
}
