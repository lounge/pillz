use spacetimedb::SpacetimeType;
#[derive(SpacetimeType, Debug, Clone, Copy, PartialEq)]
pub struct Jetpack {
    pub fuel: f32,
    pub enabled: bool,
    pub throttling: bool,
}
