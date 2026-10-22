using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollBG : MonoBehaviour
{
    public Player player;
    public float scrollFactor = 0.05f;
    private Material mat;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        mat = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 offset = mat.mainTextureOffset;
        offset.x = player.transform.position.x * scrollFactor;
        mat.mainTextureOffset = offset;
    }
}
