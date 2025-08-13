use spacetimedb::Identity;

#[spacetimedb::table(name = config, public)]
#[derive(Debug, Clone)]
pub struct Config {
    #[primary_key]
    pub id: u32,
    pub observer: Option<Identity>,
}
