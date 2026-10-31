using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCone : MonoBehaviour
    
{
    public TurretAI turret;
    public bool isLeft = false;
    // Start is called before the first frame update
    private void Awake()
    {
        turret = gameObject.GetComponentInParent<TurretAI>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (isLeft)
                turret.Attack(false);
            else
                turret.Attack(true);
        }
    }

}
