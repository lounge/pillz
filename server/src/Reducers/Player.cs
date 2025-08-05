using masks.server.Tables;
using SpacetimeDB;
using PlayerInput = masks.server.Tables.PlayerInput;

namespace masks.server.Reducers;

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
            ctx.Db.Player.Insert(new Tables.Player
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
        foreach (var mask in ctx.Db.Mask.PlayerId.Filter(player.Id))
        {
            var entity = ctx.Db.Entity.Id.Find(mask.EntityId) ?? throw new Exception("Could not find mask");
            ctx.Db.Entity.Id.Delete(entity.Id);
            ctx.Db.Mask.EntityId.Delete(entity.Id);
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

        var mask =ctx.Db.Mask.Insert(new Mask
        {
            EntityId = entity.Id,
            PlayerId = player.Id,
            Direction = new DbVector2(0, 0),
            Position = entity.Position,
            Hp = 100
        });

        Log.Info($"Spawned mask at ({mask.Position.X}, {entity.Position.Y}) with id: {entity.Id}.");
        
        Log.Info($"Spawned entity at ({entity.Position.X}, {entity.Position.Y}) with id: {entity.Id}.");
    }

    [Reducer]
    public static void UpdatePlayerInput(ReducerContext ctx, PlayerInput input)
    {
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ?? throw new Exception("Player not found");
        foreach (var m in ctx.Db.Mask.PlayerId.Filter(player.Id))
        {
            var mask = m;
            mask.Direction = input.Direction;
            mask.Position = input.Position;

            player.IsPaused = input.IsPaused;
            ctx.Db.Player.Identity.Update(player);
            ctx.Db.Mask.EntityId.Update(mask);
            // Log.Debug($"Updated mask with id {mask.EntityId} direction to ({mask.Velocity.X}, {mask.Velocity.Y}).");
        }
    }
    
    [Reducer]
    public static void ApplyDamage(ReducerContext ctx, uint playerId, uint damage)
    {
        uint fragCount = 0;
        var enemy = ctx.Db.Player.Id.Find(playerId)  ?? throw new Exception("Player not found");
        foreach (var m in ctx.Db.Mask.PlayerId.Filter(enemy.Id))
        {
            var mask = m;
            var hp = Math.Max(0, mask.Hp - damage);
            mask.Hp = hp;
            
            if (hp <= 0)
            {
                fragCount++;
            }
            
            ctx.Db.Mask.EntityId.Update(mask);
            Log.Debug($"Updated mask with id {mask.EntityId} HP to {mask.Hp} after taking damage {damage}.");
        }
        
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ??  throw new Exception("Player not found");
        foreach (var m in ctx.Db.Mask.PlayerId.Filter(player.Id))
        {
            var mask = m;
            mask.Dmg += damage;
            mask.Frags += fragCount;
            ctx.Db.Mask.EntityId.Update(mask);
            Log.Debug($"Updated mask with id {mask.EntityId} damage to {mask.Dmg} after giving damage {damage} fragCount {fragCount}.");
        }
    }

    [Reducer]
    public static void DeleteMask(ReducerContext ctx, uint? playerId = null)
    {
        var player = playerId != null ? ctx.Db.Player.Id.Find(playerId.Value) : ctx.Db.Player.Identity.Find(ctx.Sender);

        if (player == null)
        {
            throw new Exception("Player not found");
        }

        foreach (var mask in ctx.Db.Mask.PlayerId.Filter(player.Value.Id))
        {
            var entity = ctx.Db.Entity.Id.Find(mask.EntityId) ?? throw new Exception("Could not find mask");
            ctx.Db.Entity.Id.Delete(entity.Id);
            ctx.Db.Mask.EntityId.Delete(entity.Id);
            ctx.Db.Projectile.PlayerId.Delete(player.Value.Id);
        }
        
        Log.Debug($"Deleted mask with id {player.Value.Id}.");
    }
}