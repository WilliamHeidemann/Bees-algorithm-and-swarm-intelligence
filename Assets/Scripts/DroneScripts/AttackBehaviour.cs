using UnityEngine;

namespace DroneScripts
{
    public class AttackBehaviour : DroneBehaviour
    {
        private readonly Transform _playerTransform;
        private const float SeparationWeight = 100000;
        private float _laserTime;
        private const float LaserCooldown = 2f;
        private bool HuntUtility => drone.health > 50;

        public AttackBehaviour(Drone drone) : base(drone)
        {
            lineColor = Color.red;
            _playerTransform = drone.gameManager.playerDreadnaught;
            target = _playerTransform.position;
        }

        public override void Execute()
        {
            if (HuntUtility)
            {
                SetHuntPosition();
                if (LaserReady() && InAttackRange()) 
                {
                    drone.ShootPlayer();
                    _laserTime = Time.time + LaserCooldown;
                }
            }
            else
            {
                SetFleePosition();
                if (InSafety())
                {
                    motherShip.Retreat(drone);
                }
            }
            base.Execute();
        }

        private bool InAttackRange() => Vector3.Distance(drone.transform.position, _playerTransform.position) < 250f;

        private bool InSafety() => Vector3.Distance(drone.transform.position, _playerTransform.position) > 500;

        private void SetFleePosition() => target = drone.transform.position - _playerTransform.position;

        private bool LaserReady() => _laserTime < Time.time;

        private void SetHuntPosition()
        {
            var playerPosition = _playerTransform.position;
            var droneTransform = drone.transform;
            var distanceVector = droneTransform.position - playerPosition;
            var separationDirection = distanceVector.normalized;
            var offset = _playerTransform.forward * 200;
            target = playerPosition + separationDirection / distanceVector.magnitude * SeparationWeight + offset;
        }

        protected override void RotateTowardsTarget()
        {
            var transform = drone.transform;
            transform.forward = _playerTransform.position - transform.position;
        }
    }
}