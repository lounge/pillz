use spacetimedb::SpacetimeType;
#[derive(SpacetimeType, Debug, Clone, Copy, PartialEq)]
pub struct JetpackInput {
    pub fuel: f32,
    pub enabled: bool,
    pub throttling: bool,
}

impl JetpackInput {
    pub fn new(fuel: f32, enabled: bool, throttling: bool) -> Self {
        Self {
            fuel,
            enabled,
            throttling,
        }
    }
}
