using System.Linq;
using UnityEngine;

namespace DroneScripts
{
    public class ScoutingBehaviour : DroneBehaviour
    {
        private float scoutTimer;
        private float detectTimer;
        private float scoutTime = 10.0f;
        private float detectTime = 5.0f;
        private float detectionRadius = 400.0f;
        private int newResourceVal;
        private Asteroid newResourceObject;
        
        public ScoutingBehaviour(Drone drone) : base(drone)
        {
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
            if (TargetReached())
            {
                motherShip.drones.Add(drone);
                motherShip.scouts.Remove(drone);
                motherShip.resourceObjects.Add(newResourceObject);
                newResourceVal = 0;
                newResourceObject = null;
                // Tell mothership to do this, and have it send an idle state to the drone
            }
        }

        private void SeekNewAsteroid()
        {
            lineColor = Color.yellow;
            if (TargetReached())
            {
                target = NewScoutPosition();
            }

            if (Time.time > detectTimer)
            {
                if (DetectNewResources(out var newAsteroid))
                {
                    newResourceObject = newAsteroid;
                }

                detectTimer = Time.time + detectTime;
            }
        }

        private bool TargetReached()
        {
            return Vector3.Distance(drone.transform.position, target) < detectionRadius && Time.time > scoutTimer;
        }

        private Vector3 NewScoutPosition()
        {
            var position = motherShip.transform.position;
            position.x += Random.Range(-1500, 1500);
            position.y += Random.Range(-400, 400);
            position.z += Random.Range(-1500, 1500);
            scoutTimer = Time.time + scoutTime;
            return position;
        }

        private bool DetectNewResources(out Asteroid asteroid)
        {
            var nearby = Physics.SphereCastAll(drone.transform.position, detectionRadius, Vector3.zero);
            var unknownAsteroids = nearby
                .Where(hit => hit.collider.GetComponent<Asteroid>() != null)
                .Select(hit => hit.collider.GetComponent<Asteroid>())
                .Where(asteroid => !motherShip.resourceObjects.Contains(asteroid));
            asteroid = unknownAsteroids.OrderByDescending(asteroid => asteroid.resource).FirstOrDefault();
            return asteroid != null;
        }
    }
}