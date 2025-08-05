using SpacetimeDB;
using Game = pillz.server.Reducers.Game;

namespace pillz.server.Timers;

[Table(Scheduled = nameof(Game.MoveProjectiles), ScheduledAt = nameof(ScheduledAt))]
public partial struct MoveProjectilesTimer
{
    [PrimaryKey, AutoInc]
    public ulong ScheduledId;
    public ScheduleAt ScheduledAt;
}
