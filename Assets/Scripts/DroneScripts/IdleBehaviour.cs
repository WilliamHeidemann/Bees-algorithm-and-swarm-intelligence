namespace DroneScripts
{
    public class IdleBehaviour : DroneBehaviour
    {
        public IdleBehaviour(Drone drone) : base(drone)
        {
            target = drone.transform.position;
        }
    }
}