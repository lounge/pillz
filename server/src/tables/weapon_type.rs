use spacetimedb::SpacetimeType;

#[derive(SpacetimeType, Debug, Clone, Copy, PartialEq)]
pub enum WeaponType {
    None,
    Primary,
    Secondary,
}
