namespace PokaiLand.Events
{
    struct PlayerMovementEvent
    {
        public readonly float HorizontalVelocity;

        public PlayerMovementEvent(float horizontalVelocity)
        {
            this.HorizontalVelocity = horizontalVelocity;
        }
    }

    struct PlayerJumpEvent
    {
    }
    
    struct PlayerLandedEvent
    {
    }
}