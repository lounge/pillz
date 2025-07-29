using masks.server.Tables;
using SpacetimeDB;

namespace masks.server.Reducers;

public static partial class Weapon
{
    [Reducer]
    public static void ShootProjectile(ReducerContext ctx)
    {
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ?? throw new Exception("Player not found in the database.");
        
        // Create a projectile entity
        var entity = ctx.Db.Entity.Insert(new Entity
        {
            Position = new DbVector2(0, 0)
        });

        // Insert the projectile into the database
        ctx.Db.Projectile.Insert(new Projectile
        {
            EntityId = entity.Id,
            PlayerId = player.Id,
            Velocity = new DbVector2(0, 0)
        });

        Log.Info($"Player {player.Name} shot a projectile from position ({entity.Position.X}, {entity.Position.Y}).");
    }

    [Reducer]
    public static void UpdateProjectile(ReducerContext ctx)
    {
        
    }
}