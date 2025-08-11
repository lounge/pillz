using pillz.server.Tables;
using SpacetimeDB;
using DbVector2 = pillz.server.Tables.DbVector2;
using Entity = pillz.server.Tables.Entity;
using Pill = pillz.server.Tables.Pill;
using PlayerInput = pillz.server.Tables.PlayerInput;

namespace pillz.server.Reducers;

public static partial class Player
{
    [Reducer(ReducerKind.ClientConnected)]
    public static void Connect(ReducerContext ctx)
    {
        Log.Info($"{ctx.Sender} just connected to the server.");

        var player = ctx.Db.LoggedOutPlayer.Identity.Find(ctx.Sender);
        if (player != null)
        {
            ctx.Db.Player.Insert(player.Value);
            ctx.Db.LoggedOutPlayer.Identity.Delete(player.Value.Identity);
        }
        else
        {
            ctx.Db.Player.Insert(new pillz.server.Tables.Player
            {
                Identity = ctx.Sender,
                Username = "<NoName>"
            });
        }
    }

    [Reducer(ReducerKind.ClientDisconnected)]
    public static void Disconnect(ReducerContext ctx)
    {
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ?? throw new Exception("Player not found");
        foreach (var pill in ctx.Db.Pill.PlayerId.Filter(player.Id))
        {
            var entity = ctx.Db.Entity.Id.Find(pill.EntityId) ?? throw new Exception("Could not find pill");
            ctx.Db.Entity.Id.Delete(entity.Id);
            ctx.Db.Pill.EntityId.Delete(entity.Id);
            ctx.Db.Projectile.PlayerId.Delete(player.Id);
        }

        ctx.Db.LoggedOutPlayer.Insert(player);
        ctx.Db.Player.Id.Delete(player.Id);
    }

    [Reducer]
    public static void EnterGame(ReducerContext ctx, string username, DbVector2 spawnPosition)
    {
        Log.Info($"{ctx.Sender} is entering the game with name {username}.");
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ??
                     throw new Exception("Player not found in the database.");

        player.Username = username;
        ctx.Db.Player.Identity.Update(player);

        var entity = ctx.Db.Entity.Insert(new Entity
        {
            Position = spawnPosition,
        });

        var pill = ctx.Db.Pill.Insert(new Pill
        {
            EntityId = entity.Id,
            PlayerId = player.Id,
            Direction = new DbVector2(0, 0),
            Position = entity.Position,
            Hp = 100,
            PrimaryWeapon = new Tables.Weapon(),
            SecondaryWeapon = new Tables.Weapon()
        });

        Log.Info($"Spawned pill at ({pill.Position.X}, {entity.Position.Y}) with id: {entity.Id}.");
        Log.Info($"Spawned entity at ({entity.Position.X}, {entity.Position.Y}) with id: {entity.Id}.");
    }

    [Reducer]
    public static void UpdatePlayer(ReducerContext ctx, PlayerInput input)
    {
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ?? throw new Exception("Player not found");
        foreach (var p in ctx.Db.Pill.PlayerId.Filter(player.Id))
        {
            var pill = p;
            pill.Direction = input.Direction;
            pill.Position = input.Position;
            pill.SelectedWeapon = input.SelectedWeapon;
            
            player.IsPaused = input.IsPaused;
            ctx.Db.Player.Identity.Update(player);
            ctx.Db.Pill.EntityId.Update(pill);
            // Log.Debug($"Updated pill with id {pill.EntityId} direction to ({pill.Velocity.X}, {pill.Velocity.Y}).");
        }
    }

    [Reducer]
    public static void UpdateJetpack(ReducerContext ctx, JetpackInput input)
    {
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ?? throw new Exception("Player not found");
        foreach (var p in ctx.Db.Pill.PlayerId.Filter(player.Id))
        {
            var pill = p;
            pill.Jetpack.Fuel = input.Fuel;
            pill.Jetpack.Enabled = input.Enabled;
            pill.Jetpack.Throttling = input.Throttling;
            
            ctx.Db.Pill.EntityId.Update(pill);
            // Log.Debug($"Updated pill with id {pill.EntityId} direction to ({pill.Velocity.X}, {pill.Velocity.Y}).");
        }
    }
    
    [Reducer]
    public static void InitStims(ReducerContext ctx, int stims)
    {
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ?? throw new Exception("Player not found");
        foreach (var p in ctx.Db.Pill.PlayerId.Filter(player.Id))
        {
            var pill = p;
            pill.Stims = stims;
            ctx.Db.Pill.EntityId.Update(pill);
        }
    }
    
    [Reducer]
    public static void Stim(ReducerContext ctx, int strength)
    {
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ?? throw new Exception("Player not found");
        foreach (var p in ctx.Db.Pill.PlayerId.Filter(player.Id))
        {
            if (p.Stims <= 0 || p.Hp >= 100)
            {
                Log.Debug("Cannot use stim: No stims left or HP is already full.");
                return;
            }
            
            var pill = p;
            pill.Hp += strength;
            pill.Stims -= 1;
            ctx.Db.Pill.EntityId.Update(pill);
            Log.Debug($"Updated pill with id {pill.EntityId} HP to {pill.Hp} and stims to {pill.Stims} after using stim.");
        }
    }

    [Reducer]
    public static void ApplyDamage(ReducerContext ctx, uint playerId, int damage)
    {
        uint fragCount = 0;
        var enemy = ctx.Db.Player.Id.Find(playerId) ?? throw new Exception("Player not found");
        foreach (var p in ctx.Db.Pill.PlayerId.Filter(enemy.Id))
        {
            var pill = p;

            var hp = Math.Max(0, pill.Hp - damage);
            pill.Hp = hp;

            if (hp <= 0)
            {
                fragCount++;
                DeletePill(ctx, playerId);
            }
            else
            {
                ctx.Db.Pill.EntityId.Update(pill);
            }

            Log.Debug($"Updated pill with id {pill.EntityId} HP to {pill.Hp} after taking damage {damage}.");
        }

        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ?? throw new Exception("Player not found");
        foreach (var p in ctx.Db.Pill.PlayerId.Filter(player.Id))
        {
            var pill = p;
            pill.Dmg += damage;
            pill.Frags += fragCount;
            ctx.Db.Pill.EntityId.Update(pill);
            Log.Debug(
                $"Updated pill with id {pill.EntityId} damage to {pill.Dmg} after giving damage {damage} fragCount {fragCount}.");
        }
    }

    [Reducer]
    public static void ApplyForce(ReducerContext ctx, uint playerId, DbVector2? force = null)
    {
        var enemy = ctx.Db.Player.Id.Find(playerId) ?? throw new Exception("Player not found");
        foreach (var p in ctx.Db.Pill.PlayerId.Filter(enemy.Id))
        {
            var pill = p;
            pill.Force = force;

            ctx.Db.Pill.EntityId.Update(pill);
            Log.Debug($"Updated pill with id {pill.EntityId} force to ({force?.X}, {force?.Y}) after applying force.");
        }
    }

    [Reducer]
    public static void ForceApplied(ReducerContext ctx, uint playerId)
    {
        var enemy = ctx.Db.Player.Id.Find(playerId) ?? throw new Exception("Player not found");
        foreach (var p in ctx.Db.Pill.PlayerId.Filter(enemy.Id))
        {
            var pill = p;
            pill.Force = null;

            ctx.Db.Pill.EntityId.Update(pill);
            Log.Debug($"Updated pill with id {pill.EntityId} force to null after applying force.");
        }
    }

    [Reducer]
    public static void DeletePill(ReducerContext ctx, uint? playerId = null)
    {
        var player = playerId != null ? ctx.Db.Player.Id.Find(playerId.Value) : ctx.Db.Player.Identity.Find(ctx.Sender);

        if (player == null)
        {
            throw new Exception("Player not found");
        }

        foreach (var pill in ctx.Db.Pill.PlayerId.Filter(player.Value.Id))
        {
            var entity = ctx.Db.Entity.Id.Find(pill.EntityId) ?? throw new Exception("Could not find pill");
            ctx.Db.Entity.Id.Delete(entity.Id);
            ctx.Db.Pill.EntityId.Delete(entity.Id);
            ctx.Db.Projectile.PlayerId.Delete(player.Value.Id);
        }

        Log.Debug($"Deleted pill with id {player.Value.Id}.");
    }
}