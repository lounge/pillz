#[spacetimedb::table(name = world, public)]
pub struct World {
    #[primary_key]
    #[auto_inc]
    pub id: u32,

    pub width: u64,

    pub height: u64,

    pub is_generated: bool,
}