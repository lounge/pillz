use crate::tables::dbvector2::DbVector2;

#[spacetimedb::table(name = entity, public)]
#[derive(Debug, Clone)]
pub struct Entity {
    #[primary_key]
    #[auto_inc]
    pub id: u32,

    pub position: DbVector2,
}
