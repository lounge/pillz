use crate::tables::dbvector2::DbVector2;

#[spacetimedb::table(name = terrain, public)]
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Terrain {
    #[primary_key]
    #[auto_inc]
    pub id: u32,

    pub position: DbVector2,
    pub is_spawnable: bool,
}
