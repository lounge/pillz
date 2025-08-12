use spacetimedb::SpacetimeType;
use crate::math::dbvector2::DbVector2;
use crate::types::weapon_type::WeaponType;

#[derive(SpacetimeType, Debug, Clone, Copy, PartialEq)]
pub struct PlayerInput {
    pub direction: DbVector2,
    pub position: DbVector2,
    pub is_paused: bool,
    pub selected_weapon: WeaponType,
}

impl PlayerInput {
    pub fn new(
        direction: DbVector2,
        position: DbVector2,
        is_paused: bool,
        selected_weapon: WeaponType,
    ) -> Self {
        Self {
            direction,
            position,
            is_paused,
            selected_weapon,
        }
    }
}
