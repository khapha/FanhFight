using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformDown : MonoBehaviour
{
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            if (GameSession.IsPlaying && (GameInput.Down.IsPressed() || TouchButtons.DownHeld))
            {
                gameObject.GetComponent<Collider2D>().enabled = false;
                Invoke("Restore", 0.5f);
            }
        }
    }
    void Restore()
    {
        gameObject.GetComponent<Collider2D>().enabled = true;
    }
}
