using SpacetimeDB;
using Game = pillz.server.Reducers.Game;

namespace pillz.server.Timers;

[Table(Scheduled = nameof(Game.MovePlayers), ScheduledAt = nameof(ScheduledAt))]
public partial struct MovePlayersTimer
{
    [PrimaryKey, AutoInc]
    public ulong ScheduledId;
    public ScheduleAt ScheduledAt;
}
