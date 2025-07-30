using masks.server.Reducers;
using SpacetimeDB;

namespace masks.server.Timers;

[Table(Scheduled = nameof(Game.MovePlayers), ScheduledAt = nameof(ScheduledAt))]
public partial struct MovePlayersTimer
{
    [PrimaryKey, AutoInc]
    public ulong ScheduledId;
    public ScheduleAt ScheduledAt;
}
