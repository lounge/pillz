use spacetimedb::{ReducerContext, Table};
use spacetimedb::log::{debug, info, error};
use rand::Rng;
use std::collections::HashMap;
use crate::math::dbvector2::DbVector2;
use crate::tables::{
    terrain::Terrain as TerrainTile,
    portal::Portal,
};
use crate::tables::portal::portal;
use crate::tables::terrain::{terrain};
use crate::tables::world::world;

const WIDTH: usize = 300;
const HEIGHT: usize = 200;
const MARGIN: usize = 5;
const SEED_COUNT: usize = 200;
const GROWTH_ITERATIONS: usize = 4;
const CONNECT_THRESHOLD: i32 = 20;

#[spacetimedb::reducer]
pub fn add_terrain_tile(ctx: &ReducerContext, x: i32, y: i32) -> Result<(), String> {
    ctx.db.terrain().insert(TerrainTile {
        id: 0,
        position: DbVector2::new(x as f32, y as f32),
        is_spawnable: false,
    });

    Ok(())
}

#[spacetimedb::reducer]
pub fn delete_terrain_tile(ctx: &ReducerContext, x: i32, y: i32) -> Result<(), String> {
    if let Some(tile) = ctx.db.terrain().iter()
        .find(|t| t.position.x as i32 == x && t.position.y as i32 == y)
    {
        ctx.db.terrain().id().delete(tile.id);
        info!("Deleted terrain tile at ({}, {}) with id {}.", x, y, tile.id);
    }

    Ok(())
}

#[spacetimedb::reducer]
pub fn delete_terrain_tiles(ctx: &ReducerContext, x: i32, y: i32, radius: f32) -> Result<(), String> {
    let cx = x as f32;
    let cy = y as f32;
    let r2 = radius * radius;

    let tiles_to_delete: Vec<_> = ctx.db.terrain().iter()
        .filter(|t| {
            let dx = t.position.x - cx;
            let dy = t.position.y - cy;
            dx*dx + dy*dy <= r2
        })
        .collect();

    for tile in &tiles_to_delete {
        ctx.db.terrain().id().delete(tile.id);
        info!("[DeleteTerrainTiles] Deleted terrain tile at ({}, {}) with id {}.",
              tile.position.x, tile.position.y, tile.id);
    }

    debug!("[DeleteTerrainTiles] Deleted {} tiles around ({}, {}) with radius {}.",
           tiles_to_delete.len(), x, y, radius);

    Ok(())
}

#[spacetimedb::reducer]
pub fn generate_terrain(ctx: &ReducerContext, seed: i32) -> Result<(), String> {
    // Use first World row
    let mut world = if let Some(w) = ctx.db.world().iter().next() {
        w
    } else {
        error!("World is not initialized; cannot generate terrain.");
        return Ok(())
    };

    if !world.is_generated {
        generate_tiles(ctx, seed as u64);
        world.is_generated = true;
        ctx.db.world().id().update(world);
    }

    Ok(())

}

fn generate_tiles(ctx: &ReducerContext, seed: u64) {
    debug!("[GenerateTiles] Generating terrain with seed {}...", seed);

    let half_width = (WIDTH / 2) as i32;
    let half_height = (HEIGHT / 2) as i32;

    let rng = &mut ctx.rng();

    let mut terrain = vec![vec![false; HEIGHT]; WIDTH];

    // STEP 1: Natural base layer with height jitter and sine waves
    let terrain_heights = base_layer(WIDTH, rng, &mut terrain);

    // STEP 2: Place sparse random seed pixels for tunnels
    let seeds = generate_random_seeds(SEED_COUNT, rng, MARGIN, WIDTH, HEIGHT, &mut terrain);

    // STEP 3: MST to connect all seeds with Bresenham
    connect_seeds(&seeds, &mut terrain);

    // STEP 4: Tunnel drops to base only if near base
    connect_to_ground(&seeds, &terrain_heights, CONNECT_THRESHOLD, &mut terrain);

    // STEP 5: Growth iterations with 8-way neighbors and bias
    growth_generation(GROWTH_ITERATIONS, WIDTH, HEIGHT, &mut terrain, rng);

    // STEP 6: Write to database
    let mut terrain_dict: HashMap<(i32, i32), TerrainTile> = HashMap::new();

    for x in 0..WIDTH {
        for y in 0..HEIGHT {
            if terrain[x][y] {
                let pos = DbVector2::new(
                    (x as i32 - half_width) as f32,
                    (y as i32 - half_height) as f32,
                );
                let inserted = ctx.db.terrain().insert(TerrainTile {
                    id: 0,
                    position: pos,
                    is_spawnable: false,
                });
                terrain_dict.insert((pos.x as i32, pos.y as i32), inserted);
            }
        }
    }

    // STEP 7: Spawn and Portal locations
    generate_spawn_locations(ctx, WIDTH, HEIGHT, &terrain, half_width, half_height, &mut terrain_dict);
    generate_portal_locations(ctx, WIDTH, HEIGHT, &terrain, half_width, half_height);
}

fn generate_spawn_locations(
    ctx: &ReducerContext,
    width: usize,
    height: usize,
    terrain: &[Vec<bool>],
    half_width: i32,
    half_height: i32,
    terrain_dict: &mut HashMap<(i32, i32), TerrainTile>,
) {
    for x in 2..(width - 2) {
        for y in 2..(height - 3) {
            // This tile and the three above must be empty
            if terrain[x][y] || terrain[x][y + 1] || terrain[x][y + 2] || terrain[x][y + 3] {
                continue;
            }
            // Ground below
            if !terrain[x][y - 1] {
                continue;
            }

            // world position of the GROUND tile (below empty space)
            let gx = x as i32 - half_width;
            let gy = (y as i32 - 1) - half_height;
            let key = (gx, gy);

            if let Some(mut row) = terrain_dict.get(&key).cloned() {
                row.is_spawnable = true;
                ctx.db.terrain().id().update(row);
                terrain_dict.insert(key, row);
            }
        }
    }
}

fn generate_portal_locations(
    ctx: &ReducerContext,
    width: usize,
    height: usize,
    terrain: &[Vec<bool>],
    half_width: i32,
    half_height: i32,
) {
    let min_allowed_y: f32 = -40.0;

    let mut highest: Option<(f32, f32)> = None;
    let mut lowest: Option<(f32, f32)> = None;
    let mut leftest: Option<(f32, f32)> = None;
    let mut rightest: Option<(f32, f32)> = None;

    for x in 1..(width - 2) {
        for y in 2..(height - 5) {
            let space_clear = !terrain[x][y]
                && !terrain[x + 1][y]
                && !terrain[x][y + 1]
                && !terrain[x + 1][y + 1]
                && !terrain[x][y + 2]
                && !terrain[x + 1][y + 2];

            if !space_clear {
                continue;
            }

            // Ensure solid terrain directly below either side
            let has_ground_below = terrain[x][y - 1] || terrain[x + 1][y - 1];
            if !has_ground_below {
                continue;
            }

            let gx = x as i32 - half_width;
            let gy = y as i32 - half_height;

            let gx_f = gx as f32;
            let gy_f = gy as f32;

            if lowest.map_or(true, |(_, ly)| gy_f < ly) {
                lowest = Some((gx_f, gy_f));
            }
            if highest.map_or(true, |(_, hy)| gy_f > hy) {
                highest = Some((gx_f, gy_f));
            }

            if gy_f >= min_allowed_y {
                if leftest.map_or(true, |(lx, _)| gx_f < lx) {
                    leftest = Some((gx_f, gy_f));
                }
                if rightest.map_or(true, |(rx, _)| gx_f > rx) {
                    rightest = Some((gx_f, gy_f));
                }
            }
        }
    }

    if let (Some(lowest), Some(highest), Some(leftest), Some(rightest)) =
        (lowest, highest, leftest, rightest)
    {
        let mut portal1 = ctx.db.portal().insert(Portal {
            id: 0,
            connected_portal_id: 0,
            position: DbVector2::new(lowest.0, lowest.1),
        });

        let portal2 = ctx.db.portal().insert(Portal {
            id: 0,
            connected_portal_id: portal1.id,
            position: DbVector2::new(highest.0, highest.1),
        });

        let portal3 = ctx.db.portal().insert(Portal {
            id: 0,
            connected_portal_id: portal2.id,
            position: DbVector2::new(leftest.0, leftest.1),
        });

        let portal4 = ctx.db.portal().insert(Portal {
            id: 0,
            connected_portal_id: portal3.id,
            position: DbVector2::new(rightest.0, rightest.1),
        });

        // make 1 <-> 2 and 3 <-> 4
        portal1.connected_portal_id = portal2.id;
        ctx.db.portal().id().update(portal1);

        let mut portal3_updated = portal3;
        portal3_updated.connected_portal_id = portal4.id;
        ctx.db.portal().id().update(portal3_updated);
    }
}

fn connect_to_ground(
    seeds: &[(usize, usize)],
    terrain_heights: &[i32],
    connect_threshold: i32,
    terrain: &mut [Vec<bool>],
) {
    for &(x, y) in seeds {
        let base_y = terrain_heights[x];
        let y_i = y as i32;
        if y_i - base_y <= connect_threshold {
            for yy in (base_y..=y_i).rev() {
                if yy >= 0 {
                    terrain[x][yy as usize] = true;
                }
            }
        }
    }
}

fn growth_generation(
    growth_iterations: usize,
    width: usize,
    height: usize,
    terrain: &mut [Vec<bool>],
    rng: &mut impl Rng,
) {
    for _ in 0..growth_iterations {
        let mut grow_list: Vec<(usize, usize)> = Vec::new();

        for x in 1..(width - 1) {
            for y in 1..(height - 1) {
                if terrain[x][y] {
                    continue;
                }

                let mut count = 0;
                for dx in -1..=1 {
                    for dy in -1..=1 {
                        if dx == 0 && dy == 0 {
                            continue;
                        }
                        let nx = (x as isize + dx) as usize;
                        let ny = (y as isize + dy) as usize;
                        if terrain[nx][ny] {
                            count += 1;
                        }
                    }
                }

                let bias: i32 = rng.gen_range(0..=2);
                if (count as i32 + bias) >= 5 {
                    grow_list.push((x, y));

                    if count >= 6 && rng.gen::<f64>() < 0.75 {
                        for dx in -1..=1 {
                            for dy in -1..=1 {
                                let nx = x as isize + dx;
                                let ny = y as isize + dy;
                                if nx > 0 && ny > 0 && nx < width as isize && ny < height as isize {
                                    grow_list.push((nx as usize, ny as usize));
                                }
                            }
                        }
                    }
                }
            }
        }

        for (x, y) in grow_list {
            terrain[x][y] = true;
        }
    }
}

fn connect_seeds(seeds: &[(usize, usize)], terrain: &mut [Vec<bool>]) {
    if seeds.is_empty() {
        return;
    }

    let mut connected: Vec<(usize, usize)> = vec![seeds[0]];
    let mut remaining: Vec<(usize, usize)> = seeds[1..].to_vec();

    while !remaining.is_empty() {
        let mut min_dist = f64::MAX;
        let mut x1 = 0i32;
        let mut y1 = 0i32;
        let mut x2 = 0i32;
        let mut y2 = 0i32;

        for &(ax, ay) in &connected {
            for &(bx, by) in &remaining {
                let dx = ax as f64 - bx as f64;
                let dy = ay as f64 - by as f64;
                let dist = dx * dx + dy * dy;
                if dist < min_dist {
                    min_dist = dist;
                    x1 = ax as i32;
                    y1 = ay as i32;
                    x2 = bx as i32;
                    y2 = by as i32;
                }
            }
        }

        for (x, y) in bresenham_line(x1, y1, x2, y2) {
            if x >= 0 && y >= 0 {
                let xu = x as usize;
                let yu = y as usize;
                if xu < terrain.len() && yu < terrain[0].len() {
                    terrain[xu][yu] = true;
                }
            }
        }

        let target = (x2 as usize, y2 as usize);
        connected.push(target);
        if let Some(pos) = remaining.iter().position(|&p| p == target) {
            remaining.remove(pos);
        }
    }
}

fn generate_random_seeds(
    seed_count: usize,
    rng: &mut impl Rng,
    margin: usize,
    width: usize,
    height: usize,
    terrain: &mut [Vec<bool>],
) -> Vec<(usize, usize)> {
    let mut seeds = Vec::with_capacity(seed_count);
    for _ in 0..seed_count {
        let x = rng.gen_range(margin..(width - margin));
        let y = rng.gen_range(15..(height - margin));

        if !terrain[x][y] {
            terrain[x][y] = true;
            seeds.push((x, y));

            // Add chunk
            for dx in -1..=1 {
                for dy in -1..=1 {
                    let nx = x as isize + dx;
                    let ny = y as isize + dy;
                    if nx >= 0 && ny >= 0 && nx < width as isize && ny < height as isize {
                        if rng.gen::<f64>() < 0.8 {
                            terrain[nx as usize][ny as usize] = true;
                        }
                    }
                }
            }
        }
    }
    seeds
}

fn base_layer(width: usize, rng: &mut impl Rng, terrain: &mut [Vec<bool>]) -> Vec<i32> {
    let mut heights = vec![0i32; width];
    for x in 0..width {
        let noise = ((x as f64 + rng.gen::<f64>()) * 0.1).sin();
        let h = 8 + (noise * 4.0) as i32 + rng.gen_range(0..=1);
        heights[x] = h.max(0);

        for y in 0..(h.max(0) as usize) {
            if y < terrain[0].len() {
                terrain[x][y] = true;
            }
        }
    }
    heights
}

fn bresenham_line(mut x0: i32, mut y0: i32, x1: i32, y1: i32) -> Vec<(i32, i32)> {
    let mut points = Vec::new();

    let dx = (x1 - x0).abs();
    let sx = if x0 < x1 { 1 } else { -1 };
    let dy = -(y1 - y0).abs();
    let sy = if y0 < y1 { 1 } else { -1 };
    let mut err = dx + dy;

    loop {
        points.push((x0, y0));
        if x0 == x1 && y0 == y1 { break; }
        let e2 = 2 * err;
        if e2 >= dy {
            err += dy;
            x0 += sx;
        }
        if e2 <= dx {
            err += dx;
            y0 += sy;
        }
    }
    points
}
