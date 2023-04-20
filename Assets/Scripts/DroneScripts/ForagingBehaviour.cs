using UnityEngine;

namespace DroneScripts
{
    public class ForagingBehaviour : DroneBehaviour
    {
        private Asteroid _resourceToCollect;
        private bool _isResourcePickedUp;
        
        public ForagingBehaviour(Drone drone) : base(drone)
        {
            _resourceToCollect = motherShip.resourceObjects[0];
            target = _resourceToCollect.transform.position;
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
                    motherShip.idle.Add(drone);
                    motherShip.normalForagers.Remove(drone);
                    // Here handing over the resource to the mothership can be implemented
                    // Tell the mothership that we are done
                }
            }
        }
    }
}