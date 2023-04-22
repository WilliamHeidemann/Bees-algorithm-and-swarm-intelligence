using UnityEngine;

namespace DroneScripts
{
    public abstract class DroneBehaviour
    {
        protected readonly Drone drone;
        protected Vector3 target;
        protected Mothership motherShip;
        protected Color lineColor;
        protected readonly float detectionRadius = 200.0f;
        private const float Speed = 1000f;
        private const float RotationSpeed = 5.0f;
        
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
            var targetRotation = Quaternion.LookRotation(target - drone.transform.position);
            var adjRotSpeed = Mathf.Min(RotationSpeed * Time.deltaTime, 1);
            //drone.transform.rotation = Quaternion.Lerp(drone.transform.rotation, targetRotation, adjRotSpeed);
            //drone.rb.AddRelativeForce(Vector3.forward * (Speed * Time.deltaTime));
            Debug.DrawLine(drone.transform.position, target, lineColor);
        }

        protected bool TargetReached() => Vector3.Distance(drone.transform.position, target) < detectionRadius;
    }
}