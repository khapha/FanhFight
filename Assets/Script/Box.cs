using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    public int Health = 100;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    void Damage(int damage)
    {
        Health -= damage;
    }
    // Update is called once per frame
    void Update()
    {
        if(Health <= 0)
        {
            Fx.Burst(transform.position, new Color(0.6f, 0.4f, 0.2f), 20, 4f, 0.25f);
            Destroy(gameObject);
        }
    }
}
