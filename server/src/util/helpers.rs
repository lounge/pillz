use spacetimedb::{ReducerContext, Table};
use crate::tables::config::{config, Config};

pub fn get_or_create_config(ctx: &ReducerContext) -> Config {
    if let Some(cfg) = ctx.db.config().id().find(0) {
        cfg
    } else {
        let cfg = Config { id: 0, observer: None };
        ctx.db.config().insert(cfg.clone());
        cfg
    }
}

pub fn set_config(ctx: &ReducerContext, cfg: Config) {
    if ctx.db.config().id().find(0).is_some() {
        ctx.db.config().id().update(cfg);
    } else {
        ctx.db.config().insert(cfg);
    }
}