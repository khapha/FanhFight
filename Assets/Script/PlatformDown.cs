using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformDown : MonoBehaviour
{
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            if (GameInput.Down.IsPressed())
            {
                gameObject.GetComponent<Collider2D>().enabled = false;
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Invoke("Restore", 0.5f);
        }
    }
    void Restore()
    {
        gameObject.GetComponent<Collider2D>().enabled = true;
    }
}
