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

    [SerializeField] private List<Drone> idle = new();
    [SerializeField] private List<Drone> attackers = new();
    [SerializeField] private List<Drone> scouts = new();
    [SerializeField] private List<Drone> foragers = new();
    [SerializeField] private List<Drone> eliteForagers = new();
    private int _maxScouts;
    private int _maxForagers;
    private int _maxEliteForagers;
    public List<Asteroid> resourceObjects = new();
    private readonly Dictionary<Asteroid, int> _neighborhoodFitness = new();
    void Start()
    {
        const float scoutPercentage = 0.25f;
        const float foragerPercentage = 0.25f;
        const float eliteForagerPercentage = 0.25f;

        _maxScouts = Mathf.RoundToInt(numberOfEnemies * scoutPercentage);
        _maxForagers = Mathf.RoundToInt(numberOfEnemies * foragerPercentage);
        _maxEliteForagers = Mathf.RoundToInt(numberOfEnemies * eliteForagerPercentage);
        
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
        while (idle.Count > 5) // Allow some drones to refuel
        {
            var drone = Assign(idle, attackers);
            drone.droneBehaviour = new AttackBehaviour(drone);
        }
        while (scouts.Count > 0)
        {
            var drone = Assign(scouts, attackers);
            drone.droneBehaviour = new AttackBehaviour(drone);
        }
        while (foragers.Count > 0)
        {
            var drone = Assign(foragers, attackers);
            drone.droneBehaviour = new AttackBehaviour(drone);
        }
        while (eliteForagers.Count > 0)
        {
            var drone = Assign(eliteForagers, attackers);
            drone.droneBehaviour = new AttackBehaviour(drone);
        }
    }
    
    private bool ShouldRecruitScouts() => (scouts.Count < _maxScouts) && idle.Count > 0; // || resourceObjects.Count == 0

    private void RecruitScouts()
    {
        while (ShouldRecruitScouts())
        {
            var scout = Assign(idle, scouts);
            scout.droneBehaviour = new ScoutingBehaviour(scout);
        }
    }
    
    private bool ShouldRecruitEliteForagers() => eliteForagers.Count < _maxEliteForagers && idle.Count > 0 && resourceObjects.Count > 0;
    private void RecruitEliteForagers()
    {
        while (ShouldRecruitEliteForagers())
        {
            var eliteForager = Assign(idle, eliteForagers);
            var eliteBehaviour = new EliteForagingBehaviour(eliteForager);
            eliteBehaviour.SetResourceTarget(ResourceToSearch());
            eliteForager.droneBehaviour = eliteBehaviour;
        }
    }

    private Asteroid ResourceToSearch()
    {
        var roll = Random.value;
        var limit = 0.50f;
        foreach (var resource in resourceObjects)
        {
            if (roll > limit) return resource;
            limit -= 20f;
        }
        return resourceObjects[^1];
    }
    
    private bool ShouldRecruitForagers() => foragers.Count < _maxForagers && idle.Count > 0 && resourceObjects.Count > 0;
    private void RecruitForagers()
    {
        while (ShouldRecruitForagers())
        {
            var forager = Assign(idle, foragers);
            var foragingBehaviour = new ForagingBehaviour(forager);
            foragingBehaviour.SetResourceTarget(resourceObjects[0]);
            forager.droneBehaviour = foragingBehaviour;
        }
    }

    private static Drone Assign(List<Drone> from, List<Drone> to)
    {
        var drone = from.OrderByDescending(drone => drone.fuel).FirstOrDefault();
        from.Remove(drone);
        to.Add(drone);
        return drone;
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
        _neighborhoodFitness.TryAdd(asteroid, 10);
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
        if (_neighborhoodFitness[asteroidToSearchAround] > 0)
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
        var timeToSearchArea = _neighborhoodFitness[asteroidToSearchAround];
        yield return new WaitForSeconds(timeToSearchArea); // Any state change might occur at this point
        if (scoutingEliteForager.droneBehaviour is not ScoutingBehaviour scoutBehaviour) yield break; // Scout returned home
        if (scoutBehaviour.newResourceObject) yield break; // Found a new asteroid
        _neighborhoodFitness[asteroidToSearchAround] -= 1; // Neighborhood shrinking
        Swap(scoutingEliteForager, scouts, foragers);
        var foragingBehaviour = new ForagingBehaviour(scoutingEliteForager);
        foragingBehaviour.SetResourceTarget(asteroidToSearchAround);
    }

    public void ReturnToRefuel(Drone drone)
    {
        drone.droneBehaviour = new IdleBehaviour(drone);
        if (attackers.Contains(drone)) Swap(drone, attackers, idle);
        else if (scouts.Contains(drone)) Swap(drone, scouts, idle);
        else if (eliteForagers.Contains(drone)) Swap(drone, eliteForagers, idle);
        else if (foragers.Contains(drone)) Swap(drone, foragers, idle);
    }
}

