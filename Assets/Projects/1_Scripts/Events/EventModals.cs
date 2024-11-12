namespace PokaiLand.Events
{
    struct PlayerMovementEvent
    {
        public float horizontalVelocity;

        public PlayerMovementEvent(float horizontalVelocity)
        {
            this.horizontalVelocity = horizontalVelocity;
        }
    }

    struct PlayerJumpEvent
    {
    }
    
    struct PlayerLandedEvent
    {
    }
}