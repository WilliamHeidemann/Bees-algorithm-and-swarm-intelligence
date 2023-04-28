using UnityEngine;

namespace DroneScripts
{
    public class AttackBehaviour : DroneBehaviour
    {
        private readonly Transform _playerTransform;
        private const float SeparationWeight = 100000;
        
        public AttackBehaviour(Drone drone) : base(drone)
        {
            lineColor = Color.red;
            _playerTransform = drone.gameManager.playerDreadnaught;
            target = _playerTransform.position;
        }

        public override void Execute()
        {
            AttackPosition();
            base.Execute();
            if (drone.health < 10)
            {
                motherShip.Retreat(drone);
            }
        }

        private void AttackPosition()
        {
            var playerPosition = _playerTransform.position;
            var droneTransform = drone.transform;
            var distanceVector = droneTransform.position - playerPosition;
            var separationDirection = distanceVector.normalized;
            var offset = _playerTransform.forward * 200;
            target = playerPosition + separationDirection / distanceVector.magnitude * SeparationWeight + offset;
        }

        protected override void RotateTowardTarget()
        {
            var transform = drone.transform;
            transform.forward = _playerTransform.position - transform.position;
        }
    }
}