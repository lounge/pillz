use crate::tables::dbvector2::DbVector2;

#[spacetimedb::table(name = portal, public)]
pub struct Portal {
    #[primary_key]
    #[auto_inc]
    pub id: u32,

    pub connected_portal_id: u32,
    pub position: DbVector2,
}
