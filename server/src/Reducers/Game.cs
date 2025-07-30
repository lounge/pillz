using masks.server.Timers;
using SpacetimeDB;

namespace masks.server.Reducers;

public partial class Game
{
    private const float DeltaTime = 0.05f; // 50ms tick rate = 0.05s

    [Reducer]
    public static void MovePlayers(ReducerContext ctx, MovePlayersTimer timer)
    {
        var worldSize = (ctx.Db.World.Id.Find(0) ??
                         throw new Exception("Config table is empty. Please initialize the database first.")).Size;

        foreach (var mask in ctx.Db.Mask.Iter())
        {
            if (mask.IsPaused)
            {
                Log.Debug($"Mask with id {mask.EntityId} is paused, skipping movement.");
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
            var newPosition = maskEntity.Position + velocity * DeltaTime;

            maskEntity.Position.X = newPosition.X;
            maskEntity.Position.Y = mask.IsGrounded ? -2 : newPosition.Y;

            Log.Debug(
                $"Moving mask with id {mask.EntityId} to position ({maskEntity.Position.X}, {maskEntity.Position.Y})");

            ctx.Db.Entity.Id.Update(maskEntity);
        }
    }

    [Reducer]
    public static void MoveProjectiles(ReducerContext ctx, MoveProjectilesTimer timer)
    {
        var worldSize = (ctx.Db.World.Id.Find(0) ??
                         throw new Exception("Config table is empty. Please initialize the database first.")).Size;

        foreach (var projectile in ctx.Db.Projectile.Iter())
        {
            // if (projectile.IsPaused)
            // {
            //     Log.Debug($"Mask with id {projectile.EntityId} is paused, skipping movement.");
            //     continue;
            // }          

            var entity = ctx.Db.Entity.Id.Find(projectile.EntityId);
            if (entity == null)
            {
                Log.Error($"Entity with id {projectile.EntityId} not found in the database.");
                continue;
            }

            var projectileEntity = entity.Value;
            var velocity = projectile.Velocity;
            var newPosition = projectileEntity.Position + velocity * DeltaTime;

            if (!projectileEntity.Position.Equals(newPosition))
            {
                projectileEntity.Position.X = newPosition.X;
                projectileEntity.Position.Y = newPosition.Y; // projectile.IsGrounded ? -2 ; 

                Log.Debug(
                    $"Moving projectile with id {projectile.EntityId} to position ({projectileEntity.Position.X}, {projectileEntity.Position.Y})");
            }
            


            ctx.Db.Entity.Id.Update(projectileEntity);
        }
    }
}