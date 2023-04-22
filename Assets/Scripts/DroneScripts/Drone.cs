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
    public Rigidbody rb;
    public DroneBehaviour droneBehaviour;
    public float rotationSpeed = 0.1f;
    
    void Start() 
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        droneBehaviour.Execute();
        transform.forward = Vector3.Lerp(transform.forward, rb.velocity, Time.deltaTime * rotationSpeed);
        // transform.forward = rb.velocity;
        // transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rb.velocity), Mathf.Min(5f * Time.deltaTime, 1));

        // var targetRotation = Quaternion.LookRotation(target - transform.position);
        // var adjRotSpeed = Mathf.Min(RotationSpeed * Time.deltaTime, 1);
        // transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, adjRotSpeed);
        // rb.AddRelativeForce(Vector3.forward * (Speed * Time.deltaTime));
    }
}