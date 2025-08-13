use crate::tables::ammo::ammo;
use crate::tables::entity::entity;
use crate::tables::player::player;
use log::debug;
use spacetimedb::ReducerContext;
use crate::dto::item_movement::ItemMovement;

#[spacetimedb::reducer]
pub fn update_ammo(ctx: &ReducerContext, id: u32, movement: ItemMovement) {
    let player = ctx.db
        .player()
        .identity()
        .find(&ctx.sender)
        .ok_or("Observer not found in the database.")
        .unwrap();

    if let Some(mut ammo) = ctx.db.ammo().entity_id().find(id) {
        let clone = ammo.clone();
        ammo.position = movement.position;
        ammo.direction = movement.direction;
        ctx.db.ammo().entity_id().update(ammo);

        if let Some(mut e) = ctx.db.entity().id().find(id) {
            e.position = movement.position;
            ctx.db.entity().id().update(e);
        }

        debug!(
            "({})Updated ammo type ({:?}) at ({}, {}) with id: {}.",
            player.id, clone.ammo_type, clone.position.x, clone.position.y, clone.entity_id
        );
    }
}

#[spacetimedb::reducer]
pub fn delete_ammo(ctx: &ReducerContext, id: u32) {
    let ammo = ctx.db.ammo().entity_id().find(id).expect("Ammo not found");

    ctx.db.entity().id().delete(id);
    ctx.db.ammo().entity_id().delete(ammo.entity_id);
}
