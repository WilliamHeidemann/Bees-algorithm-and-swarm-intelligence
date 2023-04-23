using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DroneScripts
{
    public class ScoutingBehaviour : DroneBehaviour
    {
        public Asteroid newResourceObject;
        private readonly Collider[] _colliders = new Collider[4];
        private static readonly int AsteroidLayerMask = 1 << LayerMask.NameToLayer("Asteroid");
        
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
            var hits = Physics.OverlapSphereNonAlloc(drone.transform.position, DetectionRadius, _colliders, AsteroidLayerMask);
            asteroid = null;
            if (hits == 0) return false;
            var unknownAsteroids = _colliders // nearby.
                .Where(collider => collider != null)
                .Select(hit => hit.GetComponent<Asteroid>())
                .Where(asteroid => asteroid != null)
                .Where(asteroid => !motherShip.resourceObjects.Contains(asteroid));
            asteroid = unknownAsteroids.OrderByDescending(asteroid => asteroid.resource).FirstOrDefault();
            return asteroid != null;
        }
    }
}