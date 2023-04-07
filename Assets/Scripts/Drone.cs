using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public enum DroneBehaviours
{
    Idle,
    Scouting,
    Foraging
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

    //Boid Steering/Flocking Variables
    public float separationDistance = 25.0f;
    public float cohesionDistance = 50.0f;
    public float separationStrength = 250.0f;
    public float cohesionStrength = 25.0f;
    private Vector3 cohesionPos = new(0f, 0f, 0f);
    private int boidIndex = 0;

    public DroneBehaviours droneBehaviour;
    public GameObject motherShip;
    public Vector3 scoutPosition;
    
    private float scoutTimer;
    private float detectTimer;
    private float scoutTime = 10.0f;
    private float detectTime = 5.0f;
    private float detectionRadius = 400.0f;
    private int newResourceVal;
    public GameObject newResourceObject;
    
    // Use this for initialization
    void Start() {

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        rb = GetComponent<Rigidbody>();

        motherShip = gameManager.alienMothership;
        scoutPosition = motherShip.transform.position;
    }

    // Update is called once per frame
    void Update() {

        //Acquire player if spawned in
        if (gameManager.gameStarted) target = gameManager.playerDreadnaught;

        //Move towards valid targets
        if(target) MoveTowardsTarget(target.transform.position);

        BoidBehaviour();

        switch (droneBehaviour)
        {
            case DroneBehaviours.Idle:
                break;
            case DroneBehaviours.Scouting:
                Scouting();
                break;
            case DroneBehaviours.Foraging:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Scouting()
    {
        if (!newResourceObject)
        {
            if (Vector3.Distance(transform.position, scoutPosition) < detectionRadius && Time.time > scoutTimer) 
            {
                Vector3 position;
                position.x = motherShip.transform.position.x + Random.Range(-1500, 1500);
                position.y = motherShip.transform.position.y + Random.Range(-400, 400);
                position.z = motherShip.transform.position.z + Random.Range(-1500, 1500);
                scoutPosition = position;
                
                scoutTimer = Time.time + scoutTime;
            }
            else
            {
                MoveTowardsTarget(scoutPosition);
                Debug.DrawLine(transform.position, scoutPosition, Color.yellow);
            }
            
            if (Time.time > detectTimer) {
                newResourceObject = DetectNewResources();
                detectTimer = Time.time + detectTime;
            }
        }
        else
        {
            target = motherShip;
            Debug.DrawLine(transform.position, target.transform.position, Color.green);
            
            if (Vector3.Distance(transform.position, motherShip.transform.position) < targetRadius) 
            {
                motherShip.GetComponent<Mothership>().drones.Add(gameObject);
                motherShip.GetComponent<Mothership>().scouts.Remove(gameObject);
                motherShip.GetComponent<Mothership>().resourceObjects.Add(newResourceObject);
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

    private void MoveTowardsTarget(Vector3 targetPos) {
        //Rotate and move towards target if out of range
        if (Vector3.Distance(targetPos, transform.position) > targetRadius) {

            //Lerp Towards target
            targetRotation = Quaternion.LookRotation(targetPos - transform.position);
            adjRotSpeed = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, adjRotSpeed);

            rb.AddRelativeForce(Vector3.forward * (speed * 20 * Time.deltaTime));
        }
    }
    
    private void BoidBehaviour() 
    {
        boidIndex++;
        if (boidIndex >= gameManager.enemyList.Length) 
        {
            var cohesiveForce = (cohesionStrength / Vector3.Distance(cohesionPos,transform.position)) 
                                * (cohesionPos - transform.position);
            //Apply Force
            rb.AddForce(cohesiveForce);
            //Reset boidIndex
            boidIndex = 0;
            //Reset cohesion position
            cohesionPos.Set(0f, 0f, 0f);
        }

        //Currently analysed boid variables
        var pos = gameManager.enemyList[boidIndex].transform.position;
        var rot = gameManager.enemyList[boidIndex].transform.rotation;
        var dist = Vector3.Distance(transform.position, pos);
        
        //If not this boid
        if (dist > 0f) 
        {
            //If within separation
            if (dist <= separationDistance) 
            {
                //Compute scale of separation
                var scale = separationStrength / dist;
                //Apply force to ourselves
                rb.AddForce(scale * Vector3.Normalize(transform.position - pos));
            }
            
            //Otherwise if within cohesion distance of other boids
            else if (dist < cohesionDistance && dist > separationDistance) 
            {
                //Calculate the current cohesionPos
                cohesionPos += pos / gameManager.enemyList.Length;
                //Rotate slightly towards current boid
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 1f);
            }
        }
    }
}