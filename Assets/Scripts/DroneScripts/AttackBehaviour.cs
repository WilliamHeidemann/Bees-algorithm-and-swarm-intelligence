namespace DroneScripts
{
    public class AttackBehaviour : DroneBehaviour
    {
        public AttackBehaviour(Drone drone) : base(drone)
        {
        }

        public override void Execute()
        {
            target = drone.gameManager.playerDreadnaught.position;
            base.Execute();
        }
    }
}