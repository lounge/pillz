use crate::math::dbvector2::DbVector2;
use crate::types::weapon_type::WeaponType;

#[spacetimedb::table(name = ammo, public)]
#[derive(Debug, Clone)]
pub struct Ammo {
    #[primary_key]
    pub entity_id: u32,
    pub observer_id: u32,
    pub ammo_type: WeaponType,
    pub position: DbVector2,
    pub direction: DbVector2,

}
