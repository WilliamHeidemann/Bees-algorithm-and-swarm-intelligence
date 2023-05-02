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
            var offset = Random.onUnitSphere * 200;
            spawnPosition += offset;
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

    private IEnumerable<Drone> RefueledIdleDrones => idle.Where(drone => drone.fuel > 100);

    private bool ShouldRecruitAttackers() => GameManager.Instance.gameStarted;
    private void RecruitAttackers()
    {
        while (RefueledIdleDrones.Any(drone => drone.health >= 90)) // Allow some drones to refuel
        {
            var drone = RefueledIdleDrones.First(drone => drone.health >= 90);
            Swap(drone, idle, attackers);
            drone.droneBehaviour = new AttackBehaviour(drone);
        }
        while (scouts.Any())
        {
            var drone = scouts[0];
            Swap(drone, scouts, attackers);
            drone.droneBehaviour = new AttackBehaviour(drone);
        }
        while (foragers.Any())
        {
            var drone = foragers[0];
            Swap(drone, foragers, attackers);
            drone.droneBehaviour = new AttackBehaviour(drone);
        }
        while (eliteForagers.Any())
        {
            var drone = eliteForagers[0];
            Swap(drone, eliteForagers, attackers);
            drone.droneBehaviour = new AttackBehaviour(drone);
        }
    }
    
    private bool ShouldRecruitScouts() => scouts.Count < _maxScouts && RefueledIdleDrones.Any();
    
    private void RecruitScouts()
    {
        while (ShouldRecruitScouts())
        {
            var scout = BestFitScout();
            Swap(scout, idle, scouts);
            scout.droneBehaviour = new ScoutingBehaviour(scout);
        }
    }

    private Drone BestFitScout() => RefueledIdleDrones.OrderBy(drone => drone.capacity).ThenByDescending(drone => drone.fuel).First();

    private bool ShouldRecruitEliteForagers() => eliteForagers.Count < _maxEliteForagers && RefueledIdleDrones.Any() && resourceObjects.Any();
    private void RecruitEliteForagers()
    {
        while (ShouldRecruitEliteForagers())
        {
            var eliteForager = BestFitEliteForager();
            Swap(eliteForager, idle, eliteForagers);
            var eliteBehaviour = new EliteForagingBehaviour(eliteForager);
            eliteBehaviour.SetResourceTarget(ResourceToSearch(eliteForager));
            eliteForager.droneBehaviour = eliteBehaviour;
        }
    }

    private Drone BestFitEliteForager() =>
        RefueledIdleDrones.OrderByDescending(drone => drone.fuel).ThenByDescending(drone => drone.capacity).First();

    private Asteroid ResourceToSearch(Drone forager)
    {
        var roll = Random.value; 
        foreach (var resource in HeuristicResourceObjects(forager))
        {
            if (roll > 0.5f) return resource;
            roll += 0.2f;
        }
        return resourceObjects[^1]; // Pick the end of the list only if there are too few resources in the list
    }

    private List<Asteroid> HeuristicResourceObjects(Drone forager) =>
        resourceObjects.OrderByDescending(
            asteroid => asteroid.resource / Vector3.Distance(forager.transform.position, asteroid.transform.position)
            ).ToList();
    
    private bool ShouldRecruitForagers() => foragers.Count < _maxForagers && RefueledIdleDrones.Any() && resourceObjects.Any();
    private void RecruitForagers()
    {
        while (ShouldRecruitForagers())
        {
            var forager = BestFitForager(); 
            Swap(forager, idle, foragers);
            var foragingBehaviour = new ForagingBehaviour(forager);
            foragingBehaviour.SetResourceTarget(ResourceToSearch(forager));
            forager.droneBehaviour = foragingBehaviour;
        }
    }

    private Drone BestFitForager() =>
        RefueledIdleDrones.OrderByDescending(drone => drone.capacity).ThenByDescending(drone => drone.fuel).First();

    private static void Swap(Drone drone, List<Drone> from, List<Drone> to)
    {
        from.Remove(drone);
        to.Add(drone);
    }
    
    public void DiscoverResource(Asteroid asteroid, Drone scout)
    {
        if (!resourceObjects.Contains(asteroid)) resourceObjects.Add(asteroid);
        _neighborhoodFitness.TryAdd(asteroid, 10);
        Swap(scout, scouts, idle);
        scout.droneBehaviour = new IdleBehaviour(scout);
    }

    public void CollectResource(Asteroid asteroid, Drone forager)
    {
        // Here the amount of resources carried by the forager can be added to the mothership resource pool
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

    public void Retreat(Drone drone)
    {
        drone.droneBehaviour = new IdleBehaviour(drone);
        if (attackers.Contains(drone)) Swap(drone, attackers, idle);
        else if (scouts.Contains(drone)) Swap(drone, scouts, idle);
        else if (eliteForagers.Contains(drone)) Swap(drone, eliteForagers, idle);
        else if (foragers.Contains(drone)) Swap(drone, foragers, idle);
    }
}

