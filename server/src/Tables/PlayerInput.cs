namespace pillz.server.Tables;

[SpacetimeDB.Type]
public partial struct PlayerInput
{
    public DbVector2 Direction;
    public DbVector2 Position;
    public bool IsPaused;
    
    public PlayerInput(DbVector2 direction, DbVector2 position, bool isPaused)
    {
        Direction = direction;
        Position = position;
        IsPaused = isPaused;
    }
}