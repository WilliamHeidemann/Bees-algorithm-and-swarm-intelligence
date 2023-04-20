using UnityEngine;

namespace DroneScripts
{
    public class EliteForagingBehaviour : ForagingBehaviour
    {
        public EliteForagingBehaviour(Drone drone) : base(drone)
        {
        }

        public override void Execute()
        {
            if (TargetReached())
            {
                // Become a scout for 5 seconds. Tell mothership. After 5 seconds and not finding anything, become a forager
            }
            base.Execute();
        }
    }
}