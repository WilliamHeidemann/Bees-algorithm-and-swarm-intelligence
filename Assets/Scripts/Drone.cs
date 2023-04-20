using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public enum DroneBehaviours
{
    Idle,
    Scouting,
    NormalForaging,
    EliteForaging,
    Attacking
}

public class Drone : Enemy 
{
    GameManager gameManager;
    Rigidbody rb;

    //Movement & Rotation Variables
    public float speed = 50.0f;
    private float rotationSpeed = 5.0f;
    private float adjRotSpeed;
    private Quaternion targetRotation;
    public GameObject target;
    public float targetRadius = 200f;

    public DroneBehaviours droneBehaviour;
    public Mothership motherShip;
    public Vector3 scoutPosition;
    
    private float scoutTimer;
    private float detectTimer;
    private float scoutTime = 10.0f;
    private float detectTime = 5.0f;
    private float detectionRadius = 400.0f;
    private int newResourceVal;
    private GameObject newResourceObject;
    
    void Start() {

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        rb = GetComponent<Rigidbody>();

        motherShip = gameManager.alienMothership;
        scoutPosition = motherShip.transform.position;
    }

    void Update()
    {
        if (gameManager.gameStarted) droneBehaviour = DroneBehaviours.Attacking;
        switch (droneBehaviour)
        {
            case DroneBehaviours.Idle:
                break;
            case DroneBehaviours.Scouting:
                Scouting();
                break;
            case DroneBehaviours.NormalForaging:
                NormalForaging();
                break;
            case DroneBehaviours.EliteForaging:
                EliteForaging();
                break;
            case DroneBehaviours.Attacking:
                Attacking();
                break;
        }
    }

    private void EliteForaging()
    {
        if (!newResourceObject)
        {
            if (!target)
            {
                var topResourceObjects = motherShip.resourceObjects.Take(3);
                var randomIndex = Random.Range(0, topResourceObjects.Count());
                target = motherShip.resourceObjects[randomIndex];
            }
            MoveTowardsTarget(target.transform.position);
            Debug.DrawLine(transform.position, target.transform.position, Color.blue);
            if (Vector3.Distance(target.transform.position, transform.position) < targetRadius)
            {
                newResourceObject = target;
            }

            if (Time.time > detectTimer) 
            {
                newResourceObject = DetectNewResources();
                detectTimer = Time.time + detectTime;
            } // Elite behaviour
        }
        else
        {
            target = motherShip.gameObject;
            MoveTowardsTarget(target.transform.position);
            Debug.DrawLine(transform.position, target.transform.position, Color.blue);
            if (Vector3.Distance(transform.position, motherShip.transform.position) < targetRadius) 
            {
                motherShip.drones.Add(this);
                motherShip.normalForagers.Remove(this);
                target = null;
                newResourceObject = null; // Here handing over the resource to the mothership can be implemented
                droneBehaviour = DroneBehaviours.Idle;
            }
        }
    }

    private void NormalForaging()
    {
        if (!newResourceObject)
        {
            if(!target) target = motherShip.resourceObjects[0].gameObject;
            MoveTowardsTarget(target.transform.position);
            Debug.DrawLine(transform.position, target.transform.position, Color.blue);
            if (Vector3.Distance(target.transform.position, transform.position) < targetRadius)
            {
                newResourceObject = target;
            }
        }
        else
        {
            target = motherShip.gameObject;
            MoveTowardsTarget(target.transform.position);
            Debug.DrawLine(transform.position, target.transform.position, Color.blue);
            if (Vector3.Distance(transform.position, motherShip.transform.position) < targetRadius) 
            {
                motherShip.drones.Add(this);
                motherShip.normalForagers.Remove(this);
                target = null;
                newResourceObject = null; // Here handing over the resource to the mothership can be implemented
                droneBehaviour = DroneBehaviours.Idle;
            }
        }
    }

    private void Attacking()
    {
        target = gameManager.playerDreadnaught;
        MoveTowardsTarget(target.transform.position);
    }

    private void Scouting()
    {
        if (!newResourceObject)
        {
            if (Vector3.Distance(transform.position, scoutPosition) < detectionRadius && Time.time > scoutTimer) 
            {
                var position = motherShip.transform.position;
                position.x += Random.Range(-1500, 1500);
                position.y += Random.Range(-400, 400);
                position.z += Random.Range(-1500, 1500);
                scoutPosition = position;
                scoutTimer = Time.time + scoutTime;
            }
            else
            {
                MoveTowardsTarget(scoutPosition);
                Debug.DrawLine(transform.position, scoutPosition, Color.yellow);
            }
            
            if (Time.time > detectTimer) 
            {
                newResourceObject = DetectNewResources();
                detectTimer = Time.time + detectTime;
            }
        }
        else
        {
            target = motherShip.gameObject;
            MoveTowardsTarget(target.transform.position);
            Debug.DrawLine(transform.position, target.transform.position, Color.green);
            
            if (Vector3.Distance(transform.position, motherShip.transform.position) < targetRadius) 
            {
                motherShip.drones.Add(this);
                motherShip.scouts.Remove(this);
                motherShip.resourceObjects.Add(newResourceObject);
                newResourceVal = 0;
                newResourceObject = null;
                droneBehaviour = DroneBehaviours.Idle;
            }
        }
    }

    private GameObject DetectNewResources()
    {
        for (int i = 0; i < gameManager.asteroids.Length; i++)
        {
            if (Vector3.Distance(transform.position, gameManager.asteroids[i].transform.position) <= detectionRadius)
            {
                if (gameManager.asteroids[i].GetComponent<Asteroid>().resource > newResourceVal)
                {
                    newResourceObject = gameManager.asteroids[i];
                }
            }
        }
        if (motherShip.GetComponent<Mothership>().resourceObjects.Contains(newResourceObject)) 
        {
            return null;
        }
        else
        {
            return newResourceObject;
        }
    }

    private void MoveTowardsTarget(Vector3 targetPos) 
    {
        if (Vector3.Distance(targetPos, transform.position) > targetRadius) 
        {
            targetRotation = Quaternion.LookRotation(targetPos - transform.position);
            adjRotSpeed = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, adjRotSpeed);
            rb.AddRelativeForce(Vector3.forward * (speed * 20 * Time.deltaTime));
        }
    }
}