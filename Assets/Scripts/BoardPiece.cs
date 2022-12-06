using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardPiece : MonoBehaviour
{
    public ParticleSystem poof;
    public AudioSource thud;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Square") return;

        poof.Play();
        Debug.Log("Play Poof");
        thud.Play();
    }
}
