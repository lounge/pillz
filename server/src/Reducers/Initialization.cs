using pillz.server.Timers;
using SpacetimeDB;
using MovePlayersTimer = pillz.server.Timers.MovePlayersTimer;
using MoveProjectilesTimer = pillz.server.Timers.MoveProjectilesTimer;
using World = pillz.server.Tables.World;

namespace pillz.server.Reducers;

public partial class Initialization
{
    // Note the `init` parameter passed to the reducer macro.
    // That indicates to SpacetimeDB that it should be called
    // once upon database creation.
    [Reducer(ReducerKind.Init)]
    public static void Init(ReducerContext ctx)
    {
        Log.Info("Initializing the database...");

        var worldWidth = 1920 * 4;
        var worldHeight = 1080 * 4;
        
        ctx.Db.World.Insert(new World
        {
            Width = (ulong)worldWidth,
            Height = (ulong)worldHeight
        });
       
        ctx.Db.MovePlayersTimer.Insert(new MovePlayersTimer
        {
            ScheduledAt = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(50))
        });

        ctx.Db.MoveProjectilesTimer.Insert(new MoveProjectilesTimer
        {
            ScheduledAt = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(50))
        });
        
        ctx.Db.SpawnAmmoTimer.Insert(new SpawnAmmoTimer
        {
            ScheduledAt = new ScheduleAt.Interval(TimeSpan.FromSeconds(10))
        });
    }
}