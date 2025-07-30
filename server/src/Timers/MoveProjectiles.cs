using masks.server.Reducers;
using SpacetimeDB;

namespace masks.server.Timers;

[Table(Scheduled = nameof(Game.MoveProjectiles), ScheduledAt = nameof(ScheduledAt))]
public partial struct MoveProjectilesTimer
{
    [PrimaryKey, AutoInc]
    public ulong ScheduledId;
    public ScheduleAt ScheduledAt;
}
