using UnityEngine;

namespace DroneScripts
{
    public class ForagingBehaviour : DroneBehaviour
    {
        protected Asteroid resourceToCollect;
        private bool _isResourcePickedUp;
        
        public ForagingBehaviour(Drone drone) : base(drone)
        {
            resourceToCollect = motherShip.resourceObjects[0];
            target = resourceToCollect.transform.position;
            lineColor = Color.blue;
        }

        public override void Execute()
        {
            Foraging();
            base.Execute();
        }

        private void Foraging()
        {
            if (TargetReached())
            {
                if (!_isResourcePickedUp)
                {
                    target = motherShip.transform.position;
                    _isResourcePickedUp = true;
                }
                else
                {
                    motherShip.CollectResource(resourceToCollect, drone);
                }
            }
        }

        public void SetResourceTarget(Asteroid asteroid)
        {
            resourceToCollect = asteroid;
            target = asteroid.transform.position;
        }
    }
}