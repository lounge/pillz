using masks.server.Tables;
using SpacetimeDB;

namespace masks.server.Reducers;

public static partial class Weapon
{
    [Reducer]
    public static void ShootProjectile(ReducerContext ctx, DbVector2 position)
    {
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ?? throw new Exception("Player not found in the database.");
        
        // Create a projectile entity
        var entity = ctx.Db.Entity.Insert(new Entity
        {
            Position = new DbVector2(0, 0)
        });

        // foreach (var m in ctx.Db.Mask.PlayerId.Filter(player.Id))
        // {
        //     var mask = m;
        //     mask.Projectiles.Add(new Projectile
        //     {
        //         EntityId = entity.Id,
        //         PlayerId = player.Id,
        //         Velocity = new DbVector2(0, 0)
        //     });
        //     ctx.Db.Mask.EntityId.Update(mask);
        // }

        // // Insert the projectile into the database
        ctx.Db.Projectile.Insert(new Projectile
        {
            EntityId = entity.Id,
            PlayerId = player.Id,
            Velocity = new DbVector2(position.X, position.Y)
        });

        Log.Info($"Player {player.Name} shot a projectile from position ({entity.Position.X}, {entity.Position.Y}).");

        // return entity.Id;
    }

    [Reducer]
    public static void UpdateProjectile(ReducerContext ctx, DbVector2 velocity)
    {
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ?? throw new Exception("Player not found in the database.");

        foreach (var p in ctx.Db.Projectile.PlayerId.Filter(player.Id))
        {
            var projectile = p;

            projectile.Velocity = velocity;
            ctx.Db.Projectile.EntityId.Update(projectile);
            Log.Debug($"Updated projectile with id {projectile.EntityId} direction to ({projectile.Velocity.X}, {projectile.Velocity.Y}).");
        }
    }
}