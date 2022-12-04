using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool clicked;
    public bool spawned;
    public ulong clientId;
    public int player = -1;

    private void Awake()
    {
        player = -1;
    }

    public void OnMouseOver()
    {
        if (Input.GetKey(KeyCode.Mouse0)){
            clicked = true;
            Debug.Log("Clicked tile");
        }
    }

    public void Reset()
    {
        clicked = false;
        spawned = false;
        player = -1;
    }
}
