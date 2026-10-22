using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlat : MonoBehaviour
{
    public Rigidbody2D r2;
    public float timedelay;
    public float respawnDelay = 3f;
    private Vector3 startPos;
    private Quaternion startRot;
    private RigidbodyType2D startType;
    private bool falling = false;
    // Start is called before the first frame update
    void Start()
    {
        r2 = gameObject.GetComponent<Rigidbody2D>();
        startPos = transform.position;
        startRot = transform.rotation;
        startType = r2.bodyType;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player") && !falling)
        {
            StartCoroutine(fall());
        }
    }
    IEnumerator fall()
    {
        falling = true;
        yield return new WaitForSeconds(timedelay);
        r2.bodyType = RigidbodyType2D.Dynamic;
        r2.interpolation = RigidbodyInterpolation2D.Interpolate;
        // rơi xong một lúc thì hiện lại chỗ cũ, để không bị kẹt trước hố
        yield return new WaitForSeconds(respawnDelay);
        // Clear motion while the body is still dynamic: static bodies reject velocity writes.
        r2.linearVelocity = Vector2.zero;
        r2.angularVelocity = 0;
        r2.bodyType = startType;
        transform.SetPositionAndRotation(startPos, startRot);
        falling = false;
    }
}
