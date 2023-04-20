using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DroneScripts;

public class Mothership : MonoBehaviour 
{

    public Drone enemy;
    public int numberOfEnemies = 20;

    public GameObject spawnLocation;

    public List<Drone> idle = new();
    public List<Drone> scouts = new();
    public List<Drone> normalForagers = new();
    public List<Drone> eliteForagers = new();
    public int maxScouts = 4;
    public List<Asteroid> resourceObjects = new();
    private float forageTimer;
    private float forageTime = 10.0f;
    
    void Start()
    {
        for (int i = 0; i < numberOfEnemies; i++) 
        {
            var spawnPosition = spawnLocation.transform.position;
            spawnPosition.x += Random.Range(-50, 50);
            spawnPosition.y += Random.Range(-50, 50);
            spawnPosition.z += Random.Range(-50, 50);
            var instantiatedEnemy = Instantiate(enemy, spawnPosition, spawnLocation.transform.rotation);
            instantiatedEnemy.droneBehaviour = new IdleBehaviour(instantiatedEnemy);
            idle.Add(instantiatedEnemy);
        }
    }

    void Update()
    {
        if (scouts.Count < maxScouts && idle.Count > 0)
        {
            var chosenDrone = idle[0];
            scouts.Add(chosenDrone);
            idle.Remove(chosenDrone);
            chosenDrone.droneBehaviour = new ScoutingBehaviour(chosenDrone);
        }

        if (normalForagers.Count < 5 && resourceObjects.Count > 0 && idle.Count > 0)
        {
            var chosenDrone = idle[0];
            normalForagers.Add(chosenDrone);
            idle.Remove(chosenDrone);
            chosenDrone.droneBehaviour = new ForagingBehaviour(chosenDrone);
        }
        
        if (resourceObjects.Count > 0 && Time.time > forageTimer) 
        {
            resourceObjects.Sort((a, b) => b.GetComponent<Asteroid>().resource.CompareTo(a.GetComponent<Asteroid>().resource));
            forageTimer = Time.time + forageTime;
        }
    }
}

