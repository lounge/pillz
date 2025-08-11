using pillz.server.Tables;
using SpacetimeDB;
using DbVector2 = pillz.server.Tables.DbVector2;
using Entity = pillz.server.Tables.Entity;
using Projectile = pillz.server.Tables.Projectile;

namespace pillz.server.Reducers;

public static partial class Weapon
{
    [Reducer]
    public static void SetAmmo(ReducerContext ctx, int primaryAmmo, int secondaryAmmo)
    {
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ??
                     throw new Exception("Player not found in the database.");

        foreach (var p in ctx.Db.Pill.PlayerId.Filter(player.Id))
        {
            var pill = p;
            pill.PrimaryWeapon.Ammo = primaryAmmo;
            pill.SecondaryWeapon.Ammo = secondaryAmmo;

            ctx.Db.Pill.EntityId.Update(pill);
            Log.Debug($"Set ammo for player {player.Username} to Primary: {primaryAmmo}, Secondary: {secondaryAmmo}.");
        }
    }
    
    [Reducer]
    public static void IncreaseAmmo(ReducerContext ctx, int ammo, WeaponType weaponType)
    {
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ??
                     throw new Exception("Player not found in the database.");

        foreach (var p in ctx.Db.Pill.PlayerId.Filter(player.Id))
        {
            var pill = p;
            switch (weaponType)
            {
                case WeaponType.Primary:
                    pill.PrimaryWeapon.Ammo += ammo;
                    break;
                case WeaponType.Secondary:
                    pill.SecondaryWeapon.Ammo += ammo;
                    break;
            }

            ctx.Db.Pill.EntityId.Update(pill);
            Log.Debug($"Increased ammo for player {player.Username} with weapon type {weaponType} by {ammo}.");
        }
    }


    [Reducer]
    public static void ShootProjectile(ReducerContext ctx,  DbVector2 position, float speed, WeaponType weaponType, int ammo)
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

        foreach (var p in ctx.Db.Pill.PlayerId.Filter(player.Id))
        {
            var pill = p;
            var newAmmo = Math.Max(0, ammo - 1);
            switch (weaponType)
            {
                case WeaponType.Primary:
                    pill.PrimaryWeapon.Ammo = newAmmo;
                    break;
                case WeaponType.Secondary:
                    pill.SecondaryWeapon.Ammo = newAmmo;
                    break;
            }
            
            Log.Debug($"Updated ammo for player {player.Username} with weapon type {weaponType} to {newAmmo}.");

            ctx.Db.Pill.EntityId.Update(pill);
        }

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
        foreach (var p in ctx.Db.Pill.PlayerId.Filter(player.Id))
        {
            var pill = p;
            pill.AimDir = aimDir;

            ctx.Db.Pill.EntityId.Update(pill);
            // Log.Debug($"Updated pill with id {pill.EntityId} aim direction to ({pill.AimDir.X}, {pill.AimDir.Y}).");
        }
    }
}