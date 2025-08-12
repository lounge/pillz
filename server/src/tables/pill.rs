use crate::math::dbvector2::DbVector2;
use crate::types::jetpack::Jetpack;
use crate::types::weapon::Weapon;
use crate::types::weapon_type::WeaponType;

#[spacetimedb::table(name = pill, public)]
#[derive(Debug, Clone)]
pub struct Pill {
    #[primary_key]
    pub entity_id: u32,
    #[index(btree)]
    pub player_id: u32,
    pub direction: DbVector2,
    pub position: DbVector2,
    pub hp: i32,
    pub dmg: i32,
    pub frags: u32,
    pub jetpack: Jetpack,
    pub aim_dir: DbVector2,
    pub force: Option<DbVector2>,
    pub selected_weapon: WeaponType,
    pub primary_weapon: Weapon,
    pub secondary_weapon: Weapon,
    pub stims: i32,
}
