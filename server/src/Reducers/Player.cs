using masks.server.Tables;
using SpacetimeDB;

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
                Name = ""
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
        }
        ctx.Db.LoggedOutPlayer.Insert(player);
        ctx.Db.Player.Id.Delete(player.Id);
    }

    [Reducer]
    public static void EnterGame(ReducerContext ctx, string name)
    {
        Log.Info($"{ctx.Sender} is entering the game with name {name}.");
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ?? throw new Exception("Player not found in the database.");
        player.Name = name;
        ctx.Db.Player.Identity.Update(player);
        
        var entity = ctx.Db.Entity.Insert(new Entity
        {
            Position = new DbVector2(0, 0),
        });

        ctx.Db.Mask.Insert(new Mask
        {
            EntityId = entity.Id,
            PlayerId = player.Id,
            Direction = new DbVector2(0, 0)
        });

        Log.Info($"Spawned mask at ({entity.Position.X}, {entity.Position.Y}) with id: {entity.Id}.");
    }

    [Reducer]
    public static void UpdatePlayerInput(ReducerContext ctx, DbVector2 direction)
    {
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ?? throw new Exception("Player not found in the database.");

        foreach (var m in ctx.Db.Mask.PlayerId.Filter(player.Id))
        {
            var mask = m;
            
            
            mask.Direction = direction.Normalized;
            // circle.Speed = Math.Clamp(direction.Magnitude, 0f, 1f);
            ctx.Db.Mask.EntityId.Update(mask);
            Log.Debug($"Updated mask with id {mask.EntityId} direction to ({mask.Direction.X}, {mask.Direction.Y}).");
        }
    }
}