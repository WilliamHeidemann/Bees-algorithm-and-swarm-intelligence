using UnityEngine;

namespace DroneScripts
{
    public class ForagingBehaviour : DroneBehaviour
    {
        private Asteroid _resourceToCollect;
        private bool _isResourcePickedUp;
        protected float _pickupRadius = 200f;
        
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
            if (!_isResourcePickedUp)
            {
                if (Vector3.Distance(drone.transform.position, target) < _pickupRadius)
                {
                    target = motherShip.transform.position;
                    _isResourcePickedUp = true;
                }
            }
            else
            {
                if (Vector3.Distance(drone.transform.position, target) < _pickupRadius)
                {
                    motherShip.drones.Add(drone);
                    motherShip.normalForagers.Remove(drone);
                    // Here handing over the resource to the mothership can be implemented
                    // Tell the mothership that we are done
                }
            }
        }
    }
}