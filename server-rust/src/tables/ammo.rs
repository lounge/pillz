use crate::tables::dbvector2::DbVector2;
use crate::tables::weapon_type::WeaponType;

#[spacetimedb::table(name = ammo, public)]
pub struct Ammo {
    #[primary_key]
    pub entity_id: u32,

    pub ammo_type: WeaponType,
    pub position: DbVector2,
}
