use spacetimedb::ScheduleAt;
use crate::reducers::game::spawn_ammo;

#[spacetimedb::table(name = spawn_ammo_timer, scheduled(spawn_ammo))]
pub struct SpawnAmmoTimer {
    #[primary_key]
    #[auto_inc]
    pub scheduled_id: u64,

    pub scheduled_at: ScheduleAt,
}
