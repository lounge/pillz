using masks.server.Tables;
using masks.server.Timers;
using SpacetimeDB;

namespace masks.server.Reducers;

public partial class Initialization
{
    // Note the `init` parameter passed to the reducer macro.
    // That indicates to SpacetimeDB that it should be called
    // once upon database creation.
    [Reducer(ReducerKind.Init)]
    public static void Init(ReducerContext ctx)
    {
        Log.Info("Initializing the database...");
        ctx.Db.World.Insert(new World { Size = 1000 });
        
        ctx.Db.MoveAllPlayersTimer.Insert(new MoveAllPlayersTimer
        {
            ScheduledAt = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(50))
        });
    }

}