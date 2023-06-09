﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DroneScripts;
using Random = UnityEngine.Random;

public class Drone : Enemy 
{
    public GameManager gameManager;
    public Rigidbody rb;
    public DroneBehaviour droneBehaviour;
    public float fuel = 500f;
    public int capacity;
    public EnemyLaser enemyLaser;
    void Start() 
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        rb = GetComponent<Rigidbody>();
        capacity = Random.Range(1, 5);
    }

    void Update()
    {
        droneBehaviour.Execute();
    }

    public void ShootPlayer()
    {
        Instantiate(enemyLaser, transform.position, transform.rotation);
    }
}