use spacetimedb::log::{debug, info};
use spacetimedb::{ReducerContext, Table};

use crate::tables::entity::entity;
use crate::tables::pill::pill;
use crate::tables::player::player;
use crate::tables::projectile::projectile;
use crate::tables::{
    dbvector2::DbVector2, entity::Entity, projectile::Projectile,
    weapon_type::WeaponType,
};

#[spacetimedb::reducer]
pub fn init_ammo(
    ctx: &ReducerContext,
    primary_ammo: i32,
    secondary_ammo: i32,
) -> Result<(), String> {
    let player = ctx
        .db
        .player()
        .identity()
        .find(&ctx.sender)
        .ok_or("Player not found in the database.")?;

    for mut pill in ctx.db.pill().player_id().filter(&player.id) {
        pill.primary_weapon.ammo = primary_ammo;
        pill.secondary_weapon.ammo = secondary_ammo;
        ctx.db.pill().entity_id().update(pill);
    }

    debug!(
        "Set ammo for player {} to Primary: {}, Secondary: {}.",
        player.username, primary_ammo, secondary_ammo
    );

    Ok(())
}

#[spacetimedb::reducer]
pub fn increase_ammo(
    ctx: &ReducerContext,
    ammo: i32,
    weapon_type: WeaponType,
) -> Result<(), String> {
    let player = ctx
        .db
        .player()
        .identity()
        .find(&ctx.sender)
        .ok_or("Player not found in the database.")?;

    for mut pill in ctx.db.pill().player_id().filter(&player.id) {
        match weapon_type {
            WeaponType::Primary => pill.primary_weapon.ammo += ammo,
            WeaponType::Secondary => pill.secondary_weapon.ammo += ammo,
            WeaponType::None => { /* no-op */ }
        }
        ctx.db.pill().entity_id().update(pill);
    }

    debug!(
        "Increased ammo for player {} with weapon type {:?} by {}.",
        player.username, weapon_type, ammo
    );

    Ok(())
}

#[spacetimedb::reducer]
pub fn shoot_projectile(
    ctx: &ReducerContext,
    position: DbVector2,
    speed: f32,
    weapon_type: WeaponType,
    ammo: i32,
) -> Result<(), String> {
    let player = ctx
        .db
        .player()
        .identity()
        .find(&ctx.sender)
        .ok_or("Player not found in the database.")?;

    let entity = ctx.db.entity().insert(Entity {
        id: 0, // autoinc
        position: DbVector2::new(0.0, 0.0),
    });

    ctx.db.projectile().insert(Projectile {
        entity_id: entity.id,
        player_id: player.id,
        direction: DbVector2::new(position.x, position.y),
        position: DbVector2::new(0.0, 0.0), 
        speed,
    });

    for mut pill in ctx.db.pill().player_id().filter(&player.id) {
        let new_ammo = (ammo - 1).max(0);
        match weapon_type {
            WeaponType::Primary => pill.primary_weapon.ammo = new_ammo,
            WeaponType::Secondary => pill.secondary_weapon.ammo = new_ammo,
            WeaponType::None => { /* no-op */ }
        }
        ctx.db.pill().entity_id().update(pill);

        debug!(
            "Updated ammo for player {} with weapon type {:?} to {}.",
            player.username, weapon_type, new_ammo
        );
    }

    info!(
        "Player {} shot a projectile from position ({}, {}) with speed {}.",
        player.username, entity.position.x, entity.position.y, speed
    );

    Ok(())
}

#[spacetimedb::reducer]
pub fn update_projectile(
    ctx: &ReducerContext,
    velocity: DbVector2,
    position: DbVector2,
) -> Result<(), String> {
    let player = ctx
        .db
        .player()
        .identity()
        .find(&ctx.sender)
        .ok_or("Player not found in the database.")?;

    for mut proj in ctx.db.projectile().player_id().filter(&player.id) {
        proj.direction = velocity;
        proj.position = position;
        ctx.db.projectile().entity_id().update(proj);
        // debug!("Updated projectile {} dir ({}, {}), pos ({}, {})", proj.entity_id, velocity.x, velocity.y, position.x, position.y);
    }

    Ok(())
}

#[spacetimedb::reducer]
pub fn delete_projectile(ctx: &ReducerContext, id: u32) -> Result<(), String> {
    ctx.db.entity().id().delete(id);
    ctx.db.projectile().entity_id().delete(id);
    info!("Deleted a projectile and entity with id {}.", id);

    Ok(())
}

#[spacetimedb::reducer]
pub fn aim(ctx: &ReducerContext, aim_dir: DbVector2) -> Result<(), String> {
    let player = ctx
        .db
        .player()
        .identity()
        .find(&ctx.sender)
        .ok_or("Player not found in the database.")?;

    for mut pill in ctx.db.pill().player_id().filter(&player.id) {
        pill.aim_dir = aim_dir;
        ctx.db.pill().entity_id().update(pill);
        // debug!("Updated pill {} aim to ({}, {})", pill.entity_id, aim_dir.x, aim_dir.y);
    }

    Ok(())
}
