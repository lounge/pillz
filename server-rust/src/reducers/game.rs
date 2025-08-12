use rand::Rng;
use spacetimedb::{DbContext, ReducerContext, Table};

use crate::tables::ammo::ammo;
use crate::tables::dbvector2::DbVector2;
use crate::tables::entity::entity;
use crate::tables::pill::pill;
use crate::tables::player::player;
use crate::tables::projectile::projectile;
use crate::tables::terrain::terrain;
use crate::tables::world::world;
use crate::tables::{ammo::Ammo, entity::Entity, terrain::Terrain, weapon_type::WeaponType};
use crate::timers::{
    move_players_timer::MovePlayersTimer, move_projectiles_timer::MoveProjectilesTimer,
    spawn_ammo_timer::SpawnAmmoTimer,
};

const DELTA_TIME: f32 = 0.05; // 50ms

#[spacetimedb::reducer]
pub fn move_players(ctx: &ReducerContext, _timer: MovePlayersTimer) -> Result<(), String> {
    for pill in ctx.db.pill().iter() {
        if let Some(player) = ctx.db.player().id().find(pill.player_id) {
            if player.is_paused {
                // Log.Debug($"Pill with id {pill.EntityId} is paused, skipping movement.");
                continue;
            }
        }

        let Some(mut entity) = ctx.db.entity().id().find(pill.entity_id) else {
            // spacetime::log::error!("Entity with id {} not found in the database.", pill.entity_id);
            continue;
        };

        let direction = pill.direction;
        let new_position = pill.position + direction * DELTA_TIME;

        if entity.position != new_position {
            entity.position = new_position;
            ctx.db.entity().id().update(entity);
        }
        // spacetime::log::debug!(
        //     "Moving pill {} to position ({}, {})",
        //     pill.entity_id, entity.position.x, entity.position.y
        // );
    }

    Ok(())
}

#[spacetimedb::reducer]
pub fn move_projectiles(ctx: &ReducerContext, _timer: MoveProjectilesTimer) -> Result<(), String> {
    for projectile in ctx.db().projectile().iter() {
        let Some(mut entity) = ctx.db.entity().id().find(projectile.entity_id) else {
            // spacetime::log::error!("Entity with id {} not found in the database.", projectile.entity_id);
            continue;
        };

        let direction = projectile.direction;
        let new_position = projectile.position + direction * DELTA_TIME;

        if entity.position != new_position {
            entity.position = new_position;
            ctx.db.entity().id().update(entity);
            // debug!(
            //     "Moving projectile {} to ({}, {})",
            //     projectile.entity_id, entity.position.x, entity.position.y
            // );
        }
    }

    Ok(())
}

#[spacetimedb::reducer]
pub fn spawn_ammo(ctx: &ReducerContext, _timer: SpawnAmmoTimer) -> Result<(), String> {
    let Some(world) = ctx.db.world().iter().next() else {
        // spacetime::log::error!("World is not generated yet. Cannot spawn ammo.");
        return Ok(());
    };

    if !world.is_generated {
        // spacetime::log::error!("World is not generated yet. Cannot spawn ammo.");
        return Ok(());
    }

    const PRIMARY_MAX_COUNT: i32 = 20;
    const SECONDARY_MAX_COUNT: i32 = 10;

    let spawn_locations: Vec<Terrain> =
        ctx.db.terrain().iter().filter(|t| t.is_spawnable).collect();

    if spawn_locations.is_empty() {
        // spacetime::log::error!("No spawn locations available for ammo.");
        return Ok(());
    }

    let primary_count = ctx
        .db
        .ammo()
        .iter()
        .filter(|a| a.ammo_type == WeaponType::Primary)
        .count() as i32;

    let secondary_count = ctx
        .db
        .ammo()
        .iter()
        .filter(|a| a.ammo_type == WeaponType::Secondary)
        .count() as i32;

    if primary_count <= PRIMARY_MAX_COUNT {
        for _ in primary_count..PRIMARY_MAX_COUNT {
            spawn(ctx, &spawn_locations, WeaponType::Primary)
        }
    }

    if secondary_count <= SECONDARY_MAX_COUNT {
        for _ in secondary_count..SECONDARY_MAX_COUNT {
            spawn(ctx, &spawn_locations, WeaponType::Secondary)
        }
    }

    Ok(())
}

/// Helper that mirrors the local C# `Spawn(WeaponType type)` function.
fn spawn(ctx: &ReducerContext, spawn_locations: &[Terrain], ty: WeaponType) {
    let rng = &mut ctx.rng();
    let idx = rng.gen_range(0..spawn_locations.len());
    let spawn_loc = spawn_locations[idx].position;

    let entity = ctx.db.entity().insert(Entity {
        id: 0, // autoinc on insert
        position: DbVector2::new(spawn_loc.x, spawn_loc.y),
    });

    let _ammo = ctx.db.ammo().insert(Ammo {
        entity_id: entity.id,
        ammo_type: ty,
        position: spawn_loc,
    });

    // spacetime::log::debug!(
    //     "Spawned ammo type ({:?}) at ({}, {}) with id: {}.",
    //     ammo.ammo_type, entity.position.x, entity.position.y, entity.id
    // );
}
