using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerGM : MonoBehaviour
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

    public GameObject blueWins;
    public GameObject redWins;
    public GameObject tie;
    public Button quit;
    public GameObject playerQuit;
    public GameObject currentPlayer;

    public int score1;
    public int score2;

    public string player1Name;
    public string player2Name;

    bool playerTurn;
    bool victory;
    string winner;

    public Color PLAYER1COLOR;
    public Color PLAYER2COLOR;

    List<GameObject> boardPieces;

    private void Start()
    {
        AudioManager.am.PlayGameMusic();
        player1Name = Data.player1 ?? "Player 1";
        player2Name = Data.player2 ?? "Player 2";

        blueWins.SetActive(false);
        tie.SetActive(false);
        redWins.SetActive(false);
        playerQuit.SetActive(false);
        //currentPlayer.SetActive(false);
        SetCurrentPlayerText();
        quit.onClick.AddListener(() => { AudioManager.am.PlayClick1(); SceneManager.LoadScene("Menu", LoadSceneMode.Single); });

        player1Text.SetText(player1Name);
        player2Text.SetText(player2Name);

        playerTurn = true;
        boardPieces = new List<GameObject>();
    }

    private void SetCurrentPlayerText()
    {
        if (playerTurn)
        {
            currentPlayerText.SetText(player1Name + "'s\nTurn");
            currentPlayer.GetComponentInChildren<Image>().color = PLAYER1COLOR;
        }
        if (!playerTurn)
        {
            currentPlayerText.SetText(player2Name + "'s\nTurn");
            currentPlayer.GetComponentInChildren<Image>().color = PLAYER2COLOR;
        }
    }

    private void Update()
    {
        if (victory) return;

        if (playerTurn)
        {
            Player1Update();
        }
        else
        {
            Player2Update();
        }

        CheckVictory();

    }

    private void Player1Update()
    {
        for (int i = 0; i < squares.Length; i++)
        {
            Debug.Log("Player Update");
            Tile t = squares[i].GetComponent<Tile>();
            if (t.clicked && !t.spawned)
            {
                Debug.Log("Spawn X");
                t.spawned = true;
                t.player = 1;
                GameObject o = Instantiate(redX, spawnPoints[i].position, Quaternion.Euler(0f, 45f, 0f));
                o.GetComponent<Rigidbody>().isKinematic = false;
                boardPieces.Add(o);
                playerTurn = false;
                SetCurrentPlayerText();
                return;
            }
        }
    }

    private void Player2Update()
    {
        for (int i = 0; i < squares.Length; i++)
        {
            Debug.Log("Player Update");
            Tile t = squares[i].GetComponent<Tile>();
            if (t.clicked && !t.spawned)
            {
                Debug.Log("Spawn X");
                t.spawned = true;
                t.player = 2;
                GameObject o = Instantiate(blueO, spawnPoints[i].position, Quaternion.Euler(0f, 45f, 0f));
                o.GetComponent<Rigidbody>().isKinematic = false;
                boardPieces.Add(o);
                playerTurn = true;
                SetCurrentPlayerText();
                return;
            }
        }
    }

    //private void AIUpdate()
    //{
    //    List<int> options = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
    //    for (int i = 0; i < squares.Length; i++)
    //    {
    //        if (squares[i].GetComponent<Tile>().spawned)
    //        {
    //            options.Remove(i);
    //            Debug.Log($"Removing {i}");
    //        }
    //    }
    //    if (options.Count == 0) return;

    //    int pick = options[Random.Range(0, options.Count)];
    //    Debug.Log($"Picking {pick}");
    //    squares[pick].GetComponent<Tile>().spawned = true;
    //    squares[pick].GetComponent<Tile>().player = 2;
    //    GameObject o = Instantiate(blueO, spawnPoints[pick].position, Quaternion.Euler(0f, 0f, 0f));
    //    boardPieces.Add(o);
    //    o.GetComponent<Rigidbody>().isKinematic = false;


    //    //reset all other tiles player may have clicked
    //    foreach (var s in squares)
    //    {
    //        s.GetComponent<Tile>().clicked = false;
    //    }
    //    playerTurn = true;
    //}

    private void CheckVictory()
    {
        Debug.Log("Checking Victory");
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

        if (boardPieces.Count == 9 && !victory)
        {
            SetTie();
        }
    }

    private void SetVictory()
    {
        AudioManager.am.PlayWin();
        victory = true;
        if (playerTurn)
        {
            winner = "Player 2";
            blueWins.SetActive(true);
            score2++;
            player2Score.SetText("Score - " + score2);
            blueWins.transform.GetChild(1).GetComponent<TMP_Text>().SetText(player2Name + " Wins!");
            blueWins.GetComponentInChildren<Button>().onClick.AddListener(ResetGame);
        }
        else
        {
            winner = "Player 1";
            redWins.SetActive(true);
            score1++;
            player1Score.SetText("Score - " + score1);
            redWins.transform.GetChild(1).GetComponent<TMP_Text>().SetText(player1Name + " Wins!");
            redWins.GetComponentInChildren<Button>().onClick.AddListener(ResetGame);
        }


    }

    private void SetTie()
    {
        AudioManager.am.PlayTie();
        victory = true;
        tie.SetActive(true);
        tie.GetComponentInChildren<Button>().onClick.AddListener(ResetGame);
    }

    private void ResetGame()
    {
        AudioManager.am.PlayClick1();
        redWins.SetActive(false);
        blueWins.SetActive(false);
        tie.SetActive(false);
        foreach (GameObject o in boardPieces)
        {
            Destroy(o);
        }
        boardPieces.Clear();
        foreach (var s in squares)
        {
            s.GetComponent<Tile>().Reset();
        }
        playerTurn = true;
        victory = false;
    }


}
