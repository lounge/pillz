use crate::tables::player::{player, Player};
use crate::tables::world::{world, World};
use crate::timers::move_players_timer::move_players_timer;
use crate::timers::move_projectiles_timer::move_projectiles_timer;
use crate::timers::spawn_ammo_timer::spawn_ammo_timer;
use crate::timers::{
    move_players_timer::MovePlayersTimer, move_projectiles_timer::MoveProjectilesTimer,
    spawn_ammo_timer::SpawnAmmoTimer,
};
use crate::util::constants;
use spacetimedb::log::info;
use spacetimedb::{spacetimedb_lib::ScheduleAt, ReducerContext, Table, TimeDuration};
use std::time::Duration;

const WORLD_WIDTH: u64 = 1920 * 4;
const WORLD_HEIGHT: u64 = 1080 * 4;

#[spacetimedb::reducer(init)]
pub fn init(ctx: &ReducerContext) -> Result<(), String> {
    info!("Initializing the database...");

    ctx.db.world().insert(World {
        id: 0,
        width: WORLD_WIDTH,
        height: WORLD_HEIGHT,
        is_generated: false,
    });

    ctx.db.move_players_timer().insert(MovePlayersTimer {
        scheduled_id: 0,
        scheduled_at: ScheduleAt::Interval(TimeDuration::from_duration(Duration::from_millis(50))),
    });

    ctx.db
        .move_projectiles_timer()
        .insert(MoveProjectilesTimer {
            scheduled_id: 0,
            scheduled_at: ScheduleAt::Interval(TimeDuration::from_duration(Duration::from_millis(
                50,
            ))),
        });

    ctx.db.spawn_ammo_timer().insert(SpawnAmmoTimer {
        scheduled_id: 0,
        scheduled_at: ScheduleAt::Interval(TimeDuration::from_duration(Duration::from_secs(10))),
    });

    ctx.db.player().insert(Player {
        identity: ctx.sender,
        id: constants::PLAYER_OBSERVER_ID,
        username: "<Observer>".to_string(),
        is_paused: false,
        frags: 0,
        deaths: 0,
        dmg: 0,
        
    });

    Ok(())
}


