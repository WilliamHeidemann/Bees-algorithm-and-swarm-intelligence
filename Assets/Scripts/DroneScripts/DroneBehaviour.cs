using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DroneScripts
{
    public abstract class DroneBehaviour
    {
        protected readonly Drone drone;
        protected Vector3 target;
        protected Mothership motherShip;
        protected Color lineColor;
        protected readonly float detectionRadius = 200.0f;
        
        protected DroneBehaviour(Drone drone)
        {
            this.drone = drone;
            motherShip = Object.FindObjectOfType<Mothership>();
        }

        public virtual void Execute()
        {
            MoveTowardsTarget();
        }

        private void MoveTowardsTarget()
        {
            if (Vector3.Distance(drone.transform.position, target) < 1) return;
            var directionForce = (target - drone.transform.position).normalized * 20f;
            drone.rb.AddForce(directionForce);
            Debug.DrawLine(drone.transform.position, target, lineColor);
        }

        protected bool TargetReached() => Vector3.Distance(drone.transform.position, target) < detectionRadius;
    }
}