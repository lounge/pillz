use crate::dto::jetpack_input::JetpackInput;
use crate::dto::player_input::PlayerInput;
use crate::math::dbvector2::DbVector2;
use crate::tables::config::config;
use crate::tables::entity::entity;
use crate::tables::entity::Entity;
use crate::tables::pill::pill;
use crate::tables::pill::Pill;
use crate::tables::player::Player;
use crate::tables::player::{logged_out_player, player};
use crate::tables::projectile::projectile;
use crate::types::weapon::Weapon;
use crate::types::weapon_type::WeaponType;
use crate::util::constants;
use crate::util::helpers::{get_or_create_config, set_config};
use spacetimedb::log::{debug, info};
use spacetimedb::{Identity, ReducerContext, Table};

#[spacetimedb::reducer(client_connected)]
pub fn connect(ctx: &ReducerContext) -> Result<(), String> {
    info!("{} just connected to the server.", &ctx.sender);

    if let Some(logged_out) = ctx.db.logged_out_player().identity().find(&ctx.sender) {
        ctx.db.player().insert(logged_out.clone());
        ctx.db
            .logged_out_player()
            .identity()
            .delete(logged_out.identity);
    } else {
        ctx.db.player().insert(Player {
            identity: ctx.sender,
            id: 0,
            username: "<NoName>".to_string(),
            is_paused: false,
            frags: 0,
            deaths: 0,
            dmg: 0
        });
    }

    let mut cfg = get_or_create_config(ctx);
    if cfg.observer.is_none() {
        cfg.observer = Some(ctx.sender);
        set_config(ctx, cfg);
        info!("Observer set to first player: {}", &ctx.sender);
    }

    Ok(())
}

#[spacetimedb::reducer(client_disconnected)]
pub fn disconnect(ctx: &ReducerContext) -> Result<(), String> {
    let player = ctx
        .db
        .player()
        .identity()
        .find(&ctx.sender)
        .ok_or("Player not found")?;

    for pill in ctx.db.pill().player_id().filter(player.id) {
        let entity = ctx
            .db
            .entity()
            .id()
            .find(pill.entity_id)
            .ok_or("Could not find pill")?;

        ctx.db.entity().id().delete(entity.id);
        ctx.db.pill().entity_id().delete(entity.id);
        ctx.db.projectile().player_id().delete(player.id);
    }

    ctx.db.logged_out_player().insert(player);
    ctx.db.player().identity().delete(&ctx.sender);

    if let Some(mut cfg) = ctx.db.config().id().find(0) {
        if cfg.observer == Some(ctx.sender) {
            let next_player = ctx
                .db
                .player()
                .iter()
                .find(|p| p.id != constants::PLAYER_OBSERVER_ID && p.identity != ctx.sender);

            if let Some(p) = next_player {
                cfg.observer = Some(p.identity);
                info!("Observer reassigned to {}", p.identity);
            } else {
                cfg.observer = None;
                info!("Observer cleared (no eligible players online).");
            }
            set_config(ctx, cfg);
        }
    }

    Ok(())
}

#[spacetimedb::reducer]
pub fn enter_game(
    ctx: &ReducerContext,
    username: String,
    spawn_position: DbVector2,
) -> Result<(), String> {
    info!(
        "{} is entering the game with name {}.",
        &ctx.sender, username
    );

    let mut player = ctx
        .db
        .player()
        .identity()
        .find(&ctx.sender)
        .ok_or("Player not found in the database.")?;

    let player_id = player.id;
    player.username = username;
    ctx.db.player().identity().update(player);

    let entity = ctx.db.entity().insert(Entity {
        id: 0,
        position: spawn_position,
    });

    let pill = ctx.db.pill().insert(Pill {
        entity_id: entity.id,
        player_id,
        direction: DbVector2::new(0.0, 0.0),
        position: entity.position,
        hp: 100,
        jetpack: crate::types::jetpack::Jetpack {
            fuel: 0.0,
            enabled: false,
            throttling: false,
        },
        aim_dir: DbVector2::new(0.0, 0.0),
        force: None,
        selected_weapon: WeaponType::None,
        primary_weapon: Weapon { ammo: 0 },
        secondary_weapon: Weapon { ammo: 0 },
        stims: 0,
        used_stim: false
    });

    info!(
        "Spawned pill at ({}, {}) with id: {}.",
        pill.position.x, entity.position.y, entity.id
    );
    info!(
        "Spawned entity at ({}, {}) with id: {}.",
        entity.position.x, entity.position.y, entity.id
    );

    Ok(())
}

#[spacetimedb::reducer]
pub fn update_player(ctx: &ReducerContext, input: PlayerInput) -> Result<(), String> {
    let mut player = ctx
        .db
        .player()
        .identity()
        .find(&ctx.sender)
        .ok_or("Player not found in the database.")?;

    for mut pill in ctx.db.pill().player_id().filter(&player.id) {
        pill.direction = input.direction;
        pill.position = input.position;
        pill.selected_weapon = input.selected_weapon;
        pill.used_stim = false;

        ctx.db.pill().entity_id().update(pill);
        // debug!("Updated pill {} dir ({}, {})", pill.entity_id, pill.direction.x, pill.direction.y);
    }

    player.is_paused = input.is_paused;
    ctx.db.player().identity().update(player);

    Ok(())
}

#[spacetimedb::reducer]
pub fn update_jetpack(ctx: &ReducerContext, input: JetpackInput) -> Result<(), String> {
    let player = ctx
        .db
        .player()
        .identity()
        .find(&ctx.sender)
        .ok_or("Player not found in the database.")?;

    for mut pill in ctx.db.pill().player_id().filter(&player.id) {
        pill.jetpack.fuel = input.fuel;
        pill.jetpack.enabled = input.enabled;
        pill.jetpack.throttling = input.throttling;

        ctx.db.pill().entity_id().update(pill);
    }

    Ok(())
}

#[spacetimedb::reducer]
pub fn init_stims(ctx: &ReducerContext, stims: i32) -> Result<(), String> {
    let player = ctx
        .db
        .player()
        .identity()
        .find(&ctx.sender)
        .ok_or("Player not found in the database.")?;

    for mut pill in ctx.db.pill().player_id().filter(&player.id) {
        pill.stims = stims;
        ctx.db.pill().entity_id().update(pill);
    }

    Ok(())
}

#[spacetimedb::reducer]
pub fn stim(ctx: &ReducerContext, strength: i32) -> Result<(), String> {
    const MAX_HP: i32 = 100;

    let player = ctx
        .db
        .player()
        .identity()
        .find(&ctx.sender)
        .ok_or("Player not found in the database.")?;

    for mut pill in ctx.db.pill().player_id().filter(&player.id) {
        if pill.stims <= 0 || pill.hp >= MAX_HP {
            debug!("Cannot use stim: No stims left or HP is already full.");
            continue;
        }

        let clone = pill.clone();
        pill.hp = (pill.hp + strength).min(MAX_HP);
        pill.stims -= 1;
        pill.used_stim = true;
        ctx.db.pill().entity_id().update(pill);

        debug!(
            "Updated pill {} HP to {} and stims to {} after using stim.",
            clone.entity_id, clone.hp, clone.stims
        );
    }

    Ok(())
}

#[spacetimedb::reducer]
pub fn apply_damage(ctx: &ReducerContext, player_id: u32, damage: i32) -> Result<(), String> {

    let enemy = ctx
        .db
        .player()
        .id()
        .find(player_id)
        .ok_or("Player not found in the database.")?;

    for mut pill in ctx.db.pill().player_id().filter(enemy.id) {
        let clone = pill.clone();

        let hp = (pill.hp - damage).max(0);
        pill.hp = hp;

        set_attacker_dmg(ctx, damage, enemy.identity);

        if hp <= 0 {
            ctx.db.pill().entity_id().update(pill);
            set_attacker_frags(ctx, enemy.identity);
            delete_pill(ctx, Some(player_id))?;
        } else {
            ctx.db.pill().entity_id().update(pill);
        }

        debug!(
            "Updated pill {} HP to {} after taking damage {}.",
            clone.entity_id, clone.hp, damage
        );
    }

    Ok(())
}


pub fn set_attacker_frags(ctx: &ReducerContext, enemy: Identity){
    let mut attacker = ctx
        .db
        .player()
        .identity()
        .find(&ctx.sender)
        .ok_or("Player not found in the database.").unwrap();

    if attacker.identity == enemy {
        debug!("Not setting frags for self-inflicted damage.");
        return;
    }
    attacker.frags += 1;
    debug!("frags {}", attacker.frags);
    ctx.db.player().id().update(attacker);
}

pub fn set_attacker_dmg(ctx: &ReducerContext, damage: i32, enemy: Identity){
    let mut attacker = ctx
        .db
        .player()
        .identity()
        .find(&ctx.sender)
        .ok_or("Player not found in the database.").unwrap();

    if attacker.identity == enemy {
        debug!("Not setting damage for self-inflicted damage.");
        return;
    }

    attacker.dmg += damage;
    debug!("dmg {}", attacker.dmg);
    ctx.db.player().id().update(attacker);
}


#[spacetimedb::reducer]
pub fn apply_force(
    ctx: &ReducerContext,
    player_id: u32,
    force: Option<DbVector2>,
) -> Result<(), String> {
    let enemy = ctx
        .db
        .player()
        .id()
        .find(player_id)
        .ok_or("Player not found in the database.")?;

    for mut pill in ctx.db.pill().player_id().filter(enemy.id) {
        let clone = pill.clone();
        pill.force = force;
        ctx.db.pill().entity_id().update(pill);

        if let Some(f) = force {
            debug!(
                "Updated pill {} force to ({}, {}) after applying force.",
                clone.entity_id, f.x, f.y
            );
        } else {
            debug!(
                "Updated pill {} force to None after applying force.",
                clone.entity_id
            );
        }
    }

    Ok(())
}

#[spacetimedb::reducer]
pub fn force_applied(ctx: &ReducerContext, player_id: u32) -> Result<(), String> {
    let enemy = ctx
        .db
        .player()
        .id()
        .find(player_id)
        .ok_or("Player not found in the database.")?;

    for mut pill in ctx.db.pill().player_id().filter(enemy.id) {
        let clone = pill.clone();
        pill.force = None;
        ctx.db.pill().entity_id().update(pill);
        debug!(
            "Updated pill {} force to null after applying force.",
            clone.entity_id
        );
    }

    Ok(())
}

#[spacetimedb::reducer]
pub fn delete_pill(ctx: &ReducerContext, player_id: Option<u32>) -> Result<(), String> {
    let mut player = if let Some(id) = player_id {
        ctx.db.player().id().find(id)
    } else {
        ctx.db.player().identity().find(&ctx.sender)
    }
    .ok_or("Player not found")?;

    for pill in ctx.db.pill().player_id().filter(player.id) {
        let entity = ctx
            .db
            .entity()
            .id()
            .find(pill.entity_id)
            .ok_or("Could not find pill")?;

        ctx.db.entity().id().delete(entity.id);
        ctx.db.pill().entity_id().delete(entity.id);
        ctx.db.projectile().player_id().delete(player.id);
    }

    debug!("deaths1 {}",  player.deaths);

    player.deaths += 1; //player.deaths.saturating_add(1);
    debug!("deaths2 {}",  player.deaths);
    debug!("Deleted pill with id {}.", player.id);

    ctx.db.player().id().update(player);


    Ok(())
}
