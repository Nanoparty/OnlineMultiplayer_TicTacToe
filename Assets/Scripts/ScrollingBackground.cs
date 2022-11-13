using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public MeshRenderer mr;
    public float scrollSpeed = 100f;

    private void Update()
    {
        float offset = Time.time * scrollSpeed;
        mr.material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
    }
}
