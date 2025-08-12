# TODO

## Map Boundaries
- [x] Detect when a pill falls off the map and kill it
- [x] Detect when a projectile falls off the map and destroy it
- [x] Player prefabs don't get destroyed correctly when remote player dies (Players dict in game manager)
- [ ] Stop player from moving outside the map
    - [x] Stop in min, max X
    - [ ] Stop in max Y

## Projectile Explosion
- [x] Explosion radius
- [x] Show explosion effect
- [x] Particles
- [x] Show explosion damage
- [ ] Play explosion sound
- [x] Apply force to player if in explosion radius
    - [x] Force applied if directly hit
    - [x] Force applied if in explosion radius
    - [x] Show knockback effect

## Project Renaming
- [x] Rename project to pillz
    - [x] Rename repo
    - [x] Rename pill classes and vars
    - [x] Rename server tables and reducers
    - [x] Rename prefabs, assets and game objects

## Win Scenario
- [ ] Create win scenario
    - [ ] WinScreen
        - [ ] Show kill count
        - [ ] Show dmg given

## Lobby System
- [ ] Create lobby
- [ ] Join lobby if game not in progress
- [ ] If game in progress show game in background
- [ ] Show players in lobby
- [ ] First player to join is host
    - [ ] Host can change map
    - [ ] Host can start game

## Damage and Kill Tracking
- [ ] Detect and store dmg given
    - [ ] Increase dmg points or new field destruct when destroying tiles?
    - [x] Show dmg given on HUD
    - [ ] Show dmg given on DeathScreen
- [ ] Detect and store kill count
    - [x] Show kill count on HUD
    - [ ] Show kill count on DeathScreen

## Terrain Generation
- [ ] Generate interesting terrain
    - [x] Bresenham's Line Algorithm
    - [x] MST to connect all seeds with Bresenham
    - [x] Find and use possible spawn locations
        - [x] Spawn location should be removed if the ground underneath is destroyed
    - [ ] Maybe make thicker

## Weapons
- [x] Increase projectile speed on left mouse hold
    - [ ] Needs tuning
- [ ] Add more weapons
    - [x] Different weapon types (e\.g\., rocket, grenade)
    - [x] Different dmg output
    - [x] Weapon switching mechanism

## Portals
- [ ] Portals
    - [x] Spawn four portals top, bottom, left, right
    - [x] Teleport pill to connected portal
    - [x] Fine tune portal spawn position
    - [ ] Make rigidbody and sync position to server

## Jetpack Movement
- [ ] Jetpack movement
    - [x] Faster more precise movement
    - [x] Sync jetpack activity to server
    - [x] Toggle jetpack on/off
    - [x] Jetpack fuel
        - [x] Show fuel in HUD
        - [x] Deplete fuel when used
        - [x] Refill fuel over time

## Multiple Pills
- [ ] Add multiple pillz per player
    - [ ] Toggle between players pillz