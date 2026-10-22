using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grounded : MonoBehaviour
{
    public MovingPlat mov;
    public Vector3 movp;
    public Player player;
    // Start is called before the first frame update
    void Start()
    {
        player = gameObject.GetComponentInParent<Player>();
    }
    void OnTriggerEnter2D(Collider2D collison)
    {
        if(collison.isTrigger == false)
            player.grounded = true;
    }
    private void OnTriggerStay2D(Collider2D collison)
    {
        if (collison.isTrigger == false || collison.CompareTag("water"))
            player.grounded = true;
        if (collison.isTrigger == false && collison.CompareTag("Movingplat"))
        {
            mov = collison.GetComponent<MovingPlat>();
            if (mov == null) return;
            movp = player.transform.position;
            movp.x += mov.speed * 1.3f;
            player.transform.position = movp;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.isTrigger == false || collision.CompareTag("water"))
            player.grounded = false;
    }
}
