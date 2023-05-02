using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLaser : Projectile
{
    // Update is called once per frame
    void Update () {
        //Projectile Movement
        transform.position += Time.deltaTime * projectileSpeed * transform.forward;
    }
    
    void OnTriggerEnter(Collider otherObject)
    {
        if (otherObject.CompareTag("Player"))
        {
            // Instantiate(hitEffect, transform.position, transform.rotation);
            // Instantiate(hitSound, transform.position, transform.rotation);
            Destroy (gameObject);
        }
    }
}
