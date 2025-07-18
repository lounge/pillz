using masks.server.Reducers;
using SpacetimeDB;

namespace masks.server.Timers;

[Table(Scheduled = nameof(Game.MoveAllPlayers), ScheduledAt = nameof(ScheduledAt))]
public partial struct MoveAllPlayersTimer
{
    [PrimaryKey, AutoInc]
    public ulong ScheduledId;
    public ScheduleAt ScheduledAt;
}
