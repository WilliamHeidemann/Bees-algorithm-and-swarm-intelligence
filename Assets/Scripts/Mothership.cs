using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DroneScripts;
using Random = UnityEngine.Random;

public class Mothership : MonoBehaviour 
{
    public Drone enemy;
    public int numberOfEnemies = 20;

    public GameObject spawnLocation;

    private List<Drone> idle = new();
    private List<Drone> attackers = new();
    private List<Drone> scouts = new();
    private List<Drone> foragers = new();
    private List<Drone> eliteForagers = new();
    private int maxScouts = 4;
    private int maxForagers = 5;
    private int maxEliteForagers = 5;
    public List<Asteroid> resourceObjects = new();
    private Dictionary<Asteroid, int> neighborhoodFitness = new();
    void Start()
    {
        for (int i = 0; i < numberOfEnemies; i++) 
        {
            var spawnPosition = spawnLocation.transform.position;
            spawnPosition.x += Random.Range(-50, 50);
            spawnPosition.y += Random.Range(-50, 50);
            spawnPosition.z += Random.Range(-50, 50);
            var instantiatedEnemy = Instantiate(enemy, spawnPosition, Random.rotation);
            instantiatedEnemy.droneBehaviour = new IdleBehaviour(instantiatedEnemy);
            idle.Add(instantiatedEnemy);
        }
    }

    void Update()
    {
        if (ShouldRecruitAttackers()) RecruitAttackers();
        if (ShouldRecruitScouts()) RecruitScouts();
        if (ShouldRecruitEliteForagers()) RecruitEliteForagers();
        if (ShouldRecruitForagers()) RecruitForagers();
    }

    private bool ShouldRecruitAttackers() => GameManager.Instance.gameStarted;
    private void RecruitAttackers()
    {
        while (idle.Count > 0) Assign(idle, attackers);
        while (scouts.Count > 0) Assign(scouts, attackers);
        while (foragers.Count > 0) Assign(foragers, attackers);
        while (eliteForagers.Count > 0) Assign(eliteForagers, attackers);
        foreach (var drone in attackers)
        {
            drone.droneBehaviour = new AttackBehaviour(drone);
        }
    }
    
    private bool ShouldRecruitScouts() => (scouts.Count < maxScouts || resourceObjects.Count == 0) && idle.Count > 0;

    private void RecruitScouts()
    {
        while (ShouldRecruitScouts())
        {
            Assign(idle, scouts);
            var scout = scouts[^1];
            scout.droneBehaviour = new ScoutingBehaviour(scout);
        }
    }
    
    private bool ShouldRecruitEliteForagers() => eliteForagers.Count < maxEliteForagers && idle.Count > 0 && resourceObjects.Count > 0;
    private void RecruitEliteForagers()
    {
        while (ShouldRecruitEliteForagers())
        {
            Assign(idle, eliteForagers);
            var eliteForager = eliteForagers[^1];
            eliteForager.droneBehaviour = new EliteForagingBehaviour(eliteForager);
        }
    }
    
    private bool ShouldRecruitForagers() => foragers.Count < maxForagers && idle.Count > 0 && resourceObjects.Count > 0;
    private void RecruitForagers()
    {
        while (ShouldRecruitForagers())
        {
            Assign(idle, foragers);
            var forager = foragers[^1];
            forager.droneBehaviour = new ForagingBehaviour(forager);
        }
    }

    private static void Assign(List<Drone> from, List<Drone> to)
    {
        var drone = from[0];
        from.Remove(drone);
        to.Add(drone);
    }

    private static void Swap(Drone drone, List<Drone> from, List<Drone> to)
    {
        from.Remove(drone);
        to.Add(drone);
    }
    
    public void DiscoverResource(Asteroid asteroid, Drone scout)
    {
        resourceObjects.Add(asteroid);
        resourceObjects.Sort((a, b) => b.resource.CompareTo(a.resource));
        neighborhoodFitness.TryAdd(asteroid, 10);
        Swap(scout, scouts, idle);
        scout.droneBehaviour = new IdleBehaviour(scout);
    }

    public void CollectResource(Asteroid asteroid, Drone forager)
    {
        // resource pool += asteroid.resource
        Swap(forager, foragers, idle);
        forager.droneBehaviour = new IdleBehaviour(forager);
    }

    public void InitiateEliteScouting(Drone eliteForager, Asteroid asteroidToSearchAround)
    {
        if (neighborhoodFitness[asteroidToSearchAround] > 0)
        {
            Swap(eliteForager, eliteForagers, scouts);
            eliteForager.droneBehaviour = new ScoutingBehaviour(eliteForager);
            StartCoroutine(EliteSearch(eliteForager, asteroidToSearchAround));
        }
        else
        {
            Swap(eliteForager, eliteForagers, foragers);
            var foragingBehaviour = new ScoutingBehaviour(eliteForager)
            {
                newResourceObject = asteroidToSearchAround
            };
            eliteForager.droneBehaviour = foragingBehaviour;
        }
    }

    private IEnumerator EliteSearch(Drone scoutingEliteForager, Asteroid asteroidToSearchAround)
    {
        var timeToSearchArea = neighborhoodFitness[asteroidToSearchAround];
        yield return new WaitForSeconds(timeToSearchArea);
        if (scoutingEliteForager.droneBehaviour is not ScoutingBehaviour scoutBehaviour) yield break; // Scout returned home
        if (scoutBehaviour.newResourceObject) yield break; // Found a new asteroid
        neighborhoodFitness[asteroidToSearchAround] -= 1; // Neighborhood shrinking
        Swap(scoutingEliteForager, scouts, foragers);
        scoutingEliteForager.droneBehaviour = new ForagingBehaviour(scoutingEliteForager);
    }
}

