use spacetimedb::SpacetimeType;

#[derive(SpacetimeType, Debug, Clone, Copy, PartialEq)]
pub struct Weapon {
    pub ammo: i32,
}