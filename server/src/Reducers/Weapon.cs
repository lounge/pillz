using masks.server.Tables;
using SpacetimeDB;

namespace masks.server.Reducers;

public static partial class Weapon
{
    [Reducer]
    public static void ShootProjectile(ReducerContext ctx, DbVector2 position, float speed)
    {
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ??
                     throw new Exception("Player not found in the database.");

        var entity = ctx.Db.Entity.Insert(new Entity
        {
            Position = new DbVector2(0, 0)
        });

        ctx.Db.Projectile.Insert(new Projectile
        {
            EntityId = entity.Id,
            PlayerId = player.Id,
            Direction = new DbVector2(position.X, position.Y),
            Speed = speed
        });

        Log.Info(
            $"Player {player.Username} shot a projectile from position ({entity.Position.X}, {entity.Position.Y}) and speed {speed}.");
    }

    [Reducer]
    public static void UpdateProjectile(ReducerContext ctx, DbVector2 velocity, DbVector2 position)
    {
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ?? throw new Exception("Player not found");

        foreach (var proj in ctx.Db.Projectile.PlayerId.Filter(player.Id))
        {
            var projectile = proj;

            projectile.Direction = velocity;
            projectile.Position = position;
            ctx.Db.Projectile.EntityId.Update(projectile);
            // Log.Debug($"Updated projectile with id {projectile.EntityId} direction to ({projectile.Velocity.X}, {projectile.Velocity.Y}).");
        }
    }

    [Reducer]
    public static void DeleteProjectile(ReducerContext ctx, uint id)
    {
        ctx.Db.Entity.Id.Delete(id);
        ctx.Db.Projectile.EntityId.Delete(id);
        Log.Info($"Deleted a projectile and entity with id {id}.");
    }


    [Reducer]
    public static void Aim(ReducerContext ctx, DbVector2 aimDir)
    {
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ?? throw new Exception("Player not found");
        foreach (var m in ctx.Db.Mask.PlayerId.Filter(player.Id))
        {
            var mask = m;
            mask.AimDir = aimDir;

            ctx.Db.Mask.EntityId.Update(mask);
            // Log.Debug($"Updated mask with id {mask.EntityId} aim direction to ({mask.AimDir.X}, {mask.AimDir.Y}).");
        }
    }
}