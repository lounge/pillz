use spacetimedb::ScheduleAt;
use crate::reducers::game::move_projectiles;

#[spacetimedb::table(name = move_projectiles_timer, scheduled(move_projectiles))]
pub struct MoveProjectilesTimer {
    #[primary_key]
    #[auto_inc]
    pub scheduled_id: u64,

    pub scheduled_at: ScheduleAt,
}
