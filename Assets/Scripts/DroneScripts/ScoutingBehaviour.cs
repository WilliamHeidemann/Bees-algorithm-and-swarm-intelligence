﻿using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DroneScripts
{
    public class ScoutingBehaviour : DroneBehaviour
    {
        public Asteroid newResourceObject;
        
        public ScoutingBehaviour(Drone drone) : base(drone)
        {
            target = NewScoutPosition();
        }

        public override void Execute()
        {
            Scouting();
            base.Execute();
        }

        private void Scouting()
        {
            if (!newResourceObject)
            {
                SeekNewAsteroid();
            }
            else
            {
                ReturnToMotherShip();
            }
        }

        private void ReturnToMotherShip()
        {
            target = motherShip.transform.position;
            lineColor = Color.green;
            Debug.DrawLine(drone.transform.position, newResourceObject.transform.position, lineColor);
            if (TargetReached())
            {
                motherShip.DiscoverResource(newResourceObject, drone);
            }
        }

        private void SeekNewAsteroid()
        {
            lineColor = Color.yellow;
            if (TargetReached())
            {
                target = NewScoutPosition();
            }
            
            if (DetectNewResources(out var newAsteroid))
            {
                newResourceObject = newAsteroid;
            }
        }
        
        private Vector3 NewScoutPosition()
        {
            var position = motherShip.transform.position;
            position.x += Random.Range(-1500, 1500);
            position.y += Random.Range(-400, 400);
            position.z += Random.Range(-1500, 1500);
            return position;
        }

        private bool DetectNewResources(out Asteroid asteroid)
        {
            asteroid = 
                drone.gameManager.asteroids
                    .Where(asteroid => !motherShip.resourceObjects.Contains(asteroid))
                    .FirstOrDefault(asteroid => Vector3.Distance(asteroid.transform.position, drone.transform.position) < DetectionRadius);
            return asteroid != null;
        }
    }
}