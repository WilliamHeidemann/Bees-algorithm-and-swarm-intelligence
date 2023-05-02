using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public float health = 100;

    public GameObject deathEffect;
    public GameObject deathSound;

    public virtual void TakeDamage(float dmg) 
    {
        health -= dmg;
        if (health <= 0) 
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
            Instantiate(deathSound, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
