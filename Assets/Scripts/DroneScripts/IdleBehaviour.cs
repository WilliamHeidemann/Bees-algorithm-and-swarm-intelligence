
using UnityEngine;

namespace DroneScripts
{
    public class IdleBehaviour : DroneBehaviour
    {
        private readonly Vector3 _motherShipPosition = new(-424f,160.800003f,579.599976f);
        public IdleBehaviour(Drone drone) : base(drone)
        {
            lineColor = Color.black;
        }

        public override void Execute()
        {
            target = IdleDronesTarget();
            base.Execute();
        }

        private Vector3 IdleDronesTarget()
        {
            var angle = Time.time * 0.14f + 10;
            var x = Mathf.Sin(angle);
            var z = Mathf.Cos(angle);
            var forward = new Vector3(x, 0, z);
            var idleTarget = _motherShipPosition + forward * 300f;
            //Debug.DrawLine(_motherShipPosition, idleTarget);
            return idleTarget;
        }
    }
}