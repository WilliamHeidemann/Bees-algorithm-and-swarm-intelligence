using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DroneScripts;
using Random = UnityEngine.Random;

public class Drone : Enemy 
{
    public GameManager gameManager;
    public Mothership motherShip;
    public Rigidbody rb;
    public DroneBehaviour droneBehaviour;
    
    void Start() 
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        motherShip = gameManager.alienMothership;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        droneBehaviour.Execute();
        if (gameManager.gameStarted)
        {
            // Enter attacking mode
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, 200);
    }
}