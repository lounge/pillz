using masks.server.Timers;
using SpacetimeDB;

namespace masks.server.Reducers;

public partial class Game
{
    private const float DeltaTime = 0.05f; // 50ms tick rate = 0.05s

    [Reducer]
    public static void MovePlayers(ReducerContext ctx, MovePlayersTimer timer)
    {
        // var worldSize = (ctx.Db.World.Id.Find(0) ??
        //                  throw new Exception("Config table is empty. Please initialize the database first.")).Size;

        foreach (var mask in ctx.Db.Mask.Iter())
        {
            var player = ctx.Db.Player.Id.Find(mask.PlayerId);
            if (player is { IsPaused: true })
            {
                // Log.Debug($"Mask with id {mask.EntityId} is paused, skipping movement.");
                continue;
            }

            var entity = ctx.Db.Entity.Id.Find(mask.EntityId);
            if (entity == null)
            {
                Log.Error($"Entity with id {mask.EntityId} not found in the database.");
                continue;
            }

            var maskEntity = entity.Value;
            var velocity = mask.Velocity;
            var newPosition = mask.Position + velocity * DeltaTime;


            if (!maskEntity.Position.Equals(newPosition))
            {
                maskEntity.Position.X = newPosition.X;
                maskEntity.Position.Y = newPosition.Y;
                
                ctx.Db.Entity.Id.Update(maskEntity);
                
            }
            // Log.Debug(
            //     $"Moving mask with id {mask.EntityId} to position ({maskEntity.Position.X}, {maskEntity.Position.Y})");

        }
    }

    [Reducer]
    public static void MoveProjectiles(ReducerContext ctx, MoveProjectilesTimer timer)
    {
        // var worldSize = (ctx.Db.World.Id.Find(0) ??
        //                  throw new Exception("Config table is empty. Please initialize the database first.")).Size;

        foreach (var projectile in ctx.Db.Projectile.Iter())
        {  
            var entity = ctx.Db.Entity.Id.Find(projectile.EntityId);
            if (entity == null)
            {
                Log.Error($"Entity with id {projectile.EntityId} not found in the database.");
                continue;
            }

            var projectileEntity = entity.Value;
            var velocity = projectile.Velocity;
            var newPosition = projectile.Position + velocity * DeltaTime;

            if (!projectileEntity.Position.Equals(newPosition))
            {
                projectileEntity.Position.X = newPosition.X;
                projectileEntity.Position.Y = newPosition.Y;
                
                ctx.Db.Entity.Id.Update(projectileEntity);

                // Log.Debug(
                //     $"Moving projectile with id {projectile.EntityId} to position ({projectileEntity.Position.X}, {projectileEntity.Position.Y})");
            }
        }
    }
}