use spacetimedb::SpacetimeType;
use crate::math::dbvector2::DbVector2;

#[derive(SpacetimeType, Debug, Clone, Copy, PartialEq)]
pub struct ItemMovement {
    pub position: DbVector2,
    pub direction: DbVector2,
}

impl ItemMovement {
    pub fn new(position: DbVector2, direction: DbVector2) -> Self {
        Self {
            position,
            direction,
        }
    }
}
