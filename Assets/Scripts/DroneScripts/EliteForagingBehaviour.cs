using UnityEngine;

namespace DroneScripts
{
    public class EliteForagingBehaviour : ForagingBehaviour
    {
        public EliteForagingBehaviour(Drone drone) : base(drone)
        {
            lineColor = Color.cyan;
        }

        public override void Execute()
        {
            if (TargetReached())
            {
                motherShip.InitiateEliteScouting(drone, _resourceToCollect);
            }
            base.Execute();
        }
    }
}