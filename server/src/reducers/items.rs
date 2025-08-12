use spacetimedb::ReducerContext;
use crate::math::dbvector2::DbVector2;
use crate::tables::ammo::ammo;
use crate::tables::entity::entity;

#[spacetimedb::reducer]
pub fn update_ammo(ctx: &ReducerContext, id: u32, position: DbVector2) {
    if let Some(mut entity) = ctx.db.entity().id().find(id) {
        entity.position = position;
        ctx.db.entity().id().update(entity);
    }
}

#[spacetimedb::reducer]
pub fn delete_ammo(ctx: &ReducerContext, id: u32) {
    let ammo = ctx.db.ammo().entity_id()
        .find(id)
        .expect("Ammo not found");

    ctx.db.entity().id().delete(id);
    ctx.db.ammo().entity_id().delete(ammo.entity_id);
}
