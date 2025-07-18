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
        var player = ctx.Db.Player.Identity.Find(ctx.Sender) ?? throw new Exception("Player not found in the database.");
      
        ctx.Db.LoggedOutPlayer.Insert(player);
        ctx.Db.Player.Identity.Delete(player.Identity);
    }

}