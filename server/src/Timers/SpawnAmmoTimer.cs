using SpacetimeDB;
using Game = pillz.server.Reducers.Game;

namespace pillz.server.Timers;

[Table(Scheduled = nameof(Game.SpawnPrimaryAmmo), ScheduledAt = nameof(ScheduledAt))]
public partial struct SpawnAmmoTimer
{
    [PrimaryKey, AutoInc]
    public ulong ScheduledId;
    public ScheduleAt ScheduledAt;
}

