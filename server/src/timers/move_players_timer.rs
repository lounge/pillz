use crate::reducers::game::move_players;
use spacetimedb::ScheduleAt;

#[spacetimedb::table(name = move_players_timer, scheduled(move_players))]
pub struct MovePlayersTimer {
    #[primary_key]
    #[auto_inc]
    pub scheduled_id: u64,
    pub scheduled_at: ScheduleAt,
}
