use crate::math::dbvector2::DbVector2;
#[spacetimedb::table(name = projectile, public)]
pub struct Projectile {
    #[primary_key]
    pub entity_id: u32,
    #[index(btree)]
    pub player_id: u32,
    pub direction: DbVector2,
    pub position: DbVector2,
    pub speed: f32,
}
