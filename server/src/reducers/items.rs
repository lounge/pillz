use crate::dto::item_movement::ItemMovement;
use crate::tables::ammo::ammo;
use crate::tables::entity::entity;
use spacetimedb::ReducerContext;

#[spacetimedb::reducer]
pub fn update_ammo(ctx: &ReducerContext, id: u32, movement: ItemMovement) {
    if let Some(mut ammo) = ctx.db.ammo().entity_id().find(id) {
        ammo.position = movement.position;
        ammo.direction = movement.direction;
        ctx.db.ammo().entity_id().update(ammo);

        if let Some(mut e) = ctx.db.entity().id().find(id) {
            e.position = movement.position;
            ctx.db.entity().id().update(e);
        }

        // debug!(
        //     "({})Updated ammo type ({:?}) at ({}, {}) with id: {}.",
        //     player.id, clone.ammo_type, clone.position.x, clone.position.y, clone.entity_id
        // );
    }
}

#[spacetimedb::reducer]
pub fn delete_ammo(ctx: &ReducerContext, id: u32) {
    let ammo = ctx.db.ammo().entity_id().find(id).expect("Ammo not found");

    ctx.db.entity().id().delete(id);
    ctx.db.ammo().entity_id().delete(ammo.entity_id);
}
