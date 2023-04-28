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
        }

        private void AttackPosition()
        {
            var attackPosition = _playerTransform.position;// + (_playerTransform.forward * 2 + Vector3.up) * 100;
            var distanceVector = drone.transform.position - _playerTransform.position;
            var separationDirection = distanceVector.normalized;
            target = attackPosition + (separationDirection / distanceVector.magnitude) * SeparationWeight;
        }

        protected override void RotateTowardTarget()
        {
            drone.transform.forward = _playerTransform.position - drone.transform.position;
        }
    }
}