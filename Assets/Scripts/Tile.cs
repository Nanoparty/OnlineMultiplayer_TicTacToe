using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool clicked;
    public bool spawned;

    public void OnMouseOver()
    {
        if (Input.GetKey(KeyCode.Mouse0)){
            clicked = true;
        }
    }
}
