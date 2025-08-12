using pillz.server.Tables;
using pillz.server.Timers;
using SpacetimeDB;
using MovePlayersTimer = pillz.server.Timers.MovePlayersTimer;
using MoveProjectilesTimer = pillz.server.Timers.MoveProjectilesTimer;

namespace pillz.server.Reducers;

public partial class Game
{
    private const float DeltaTime = 0.05f; // 50ms tick rate = 0.05s

    [Reducer]
    public static void MovePlayers(ReducerContext ctx, MovePlayersTimer timer)
    {
        // var worldSize = (ctx.Db.World.Id.Find(0) ??
        //                  throw new Exception("Config table is empty. Please initialize the database first.")).Size;

        foreach (var pill in ctx.Db.Pill.Iter())
        {
            var player = ctx.Db.Player.Id.Find(pill.PlayerId);
            if (player is { IsPaused: true })
            {
                // Log.Debug($"Pill with id {pill.EntityId} is paused, skipping movement.");
                continue;
            }

            var entity = ctx.Db.Entity.Id.Find(pill.EntityId);
            if (entity == null)
            {
                Log.Error($"Entity with id {pill.EntityId} not found in the database.");
                continue;
            }

            var pillEntity = entity.Value;
            var direction = pill.Direction;
            var newPosition = pill.Position + direction * DeltaTime;


            if (!pillEntity.Position.Equals(newPosition))
            {
                pillEntity.Position.X = newPosition.X;
                pillEntity.Position.Y = newPosition.Y;

                ctx.Db.Entity.Id.Update(pillEntity);
            }
            // Log.Debug(
            //     $"Moving pill with id {pill.EntityId} to position ({pillEntity.Position.X}, {pillEntity.Position.Y})");
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
            var direction = projectile.Direction;
            var newPosition = projectile.Position + direction * DeltaTime;

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

    [Reducer]
    public static void SpawnPrimaryAmmo(ReducerContext ctx, SpawnAmmoTimer timer)
    {
        var world = ctx.Db.World.Iter().FirstOrDefault();
        if (!world.IsGenerated)
        {
            Log.Error("World is not generated yet. Cannot spawn ammo.");
            return;
        }

        const int primaryMaxCount = 20;
        const int secondaryMaxCount = 10;

        var spawnLocations = ctx.Db.Terrain.Iter().Where(x => x.IsSpawnable).ToList();
        if (spawnLocations.Count == 0)
        {
            Log.Error("No spawn locations available for ammo.");
            return;
        }

        var primaryCount = ctx.Db.Ammo.Iter().Count(x => x.AmmoType == WeaponType.Primary);
        var secondaryCount = ctx.Db.Ammo.Iter().Count(x => x.AmmoType == WeaponType.Secondary);
        
        if (primaryCount <= primaryMaxCount)
        {
            for (var i = primaryCount; i < primaryMaxCount; i++)
            {
                Spawn(WeaponType.Primary);
            }
        }
        
        if (secondaryCount <= secondaryMaxCount)
        {
            for (var i = secondaryCount; i < secondaryMaxCount; i++)
            {
                Spawn(WeaponType.Secondary);
            }
        }

        void Spawn(WeaponType type)
        {
            var rnd = new Random();
            var index = rnd.Next(0, spawnLocations.Count);
            var spawnLoc = spawnLocations[index].Position;
            
            var entity = ctx.Db.Entity.Insert(new Entity
            {
                Position = new DbVector2(spawnLoc.X, spawnLoc.Y)
            });

            var ammo = ctx.Db.Ammo.Insert(new Ammo
            {
                EntityId = entity.Id,
                Position = spawnLoc,
                AmmoType = type
            });

            Log.Debug(
                $"Spawned ammo type ({ammo.AmmoType}) at ({entity.Position.X}, {entity.Position.Y}) with id: {entity.Id}.");
        }
    }
}