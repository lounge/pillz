use spacetimedb::Identity;

/// Table: Player
#[spacetimedb::table(name = player, public)]
#[spacetimedb::table(name = logged_out_player)]
#[derive(Debug, Clone)]
pub struct Player {
    #[primary_key]
    pub identity: Identity,
    #[unique]
    #[auto_inc]
    pub id: u32,
    pub username: String,
    pub is_paused: bool,
}