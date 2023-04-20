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
    private int maxForagers = 4;
    private int maxEliteForagers = 8;
    public List<Asteroid> resourceObjects = new();
    
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
        if (ShouldRecruitAttackers()) RecruitAttackers();
        if (ShouldRecruitScouts()) RecruitScouts();
        if (ShouldRecruitForagers()) RecruitForagers();
        if (ShouldRecruitEliteForagers()) RecruitEliteForagers();
    }

    private bool ShouldRecruitEliteForagers() => eliteForagers.Count < maxEliteForagers && idle.Count > 0;
    private void RecruitEliteForagers()
    {
        while (ShouldRecruitEliteForagers())
        {
            Swap(idle, eliteForagers);
            var eliteForager = eliteForagers[^1];
            eliteForager.droneBehaviour = new EliteForagingBehaviour(eliteForager);
        }
    }


    private bool ShouldRecruitForagers() => foragers.Count < maxForagers && idle.Count > 0;
    private void RecruitForagers()
    {
        while (ShouldRecruitForagers())
        {
            Swap(idle, foragers);
            var forager = foragers[^1];
            forager.droneBehaviour = new ForagingBehaviour(forager);
        }
    }


    private bool ShouldRecruitScouts() => scouts.Count < maxScouts && idle.Count > 0;

    private void RecruitScouts()
    {
        while (ShouldRecruitScouts())
        {
            Swap(idle, scouts);
            var scout = scouts[^1];
            scout.droneBehaviour = new ScoutingBehaviour(scout);
        }
    }

    private bool ShouldRecruitAttackers() => GameManager.Instance.gameStarted;
    private void RecruitAttackers()
    {
        while (idle.Count > 0) Swap(idle, attackers);
        while (scouts.Count > 0) Swap(scouts, attackers);
        while (foragers.Count > 0) Swap(foragers, attackers);
        while (eliteForagers.Count > 0) Swap(eliteForagers, attackers);
        foreach (var drone in attackers)
        {
            drone.droneBehaviour = new AttackBehaviour(drone);
        }
    }

    private void Swap(ICollection<Drone> from, ICollection<Drone> to)
    {
        var drone = idle[0];
        from.Remove(drone);
        to.Add(drone);
    }
    
    public void DiscoverResource(Asteroid asteroid, Drone scout)
    {
        resourceObjects.Add(asteroid);
        resourceObjects.Sort((a, b) => b.resource.CompareTo(a.resource));
        scouts.Remove(scout);
        idle.Add(scout);
        scout.droneBehaviour = new IdleBehaviour(scout);
    }

    public void CollectResource(Asteroid asteroid, Drone forager)
    {
        // resource pool += asteroid.resource
        foragers.Remove(forager);
        idle.Add(forager);
        forager.droneBehaviour = new IdleBehaviour(forager);
    }
}

