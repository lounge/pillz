using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
using Terrain = SpacetimeDB.Types.Terrain;

namespace pillz.client.Scripts
{
    public class GameHandler : MonoBehaviour
    {
        private const string SpacetimeDbUrl = "http://localhost:3000";
        private const string SpacetimeDbName = "pillz";

        [UsedImplicitly] private static event Action OnConnected;
        [UsedImplicitly] private static event Action OnSubscriptionApplied;

        public static GameHandler Instance { get; private set; }
        public static Identity LocalIdentity { get; private set; }
        public static DbConnection Connection { get; private set; }

        private static readonly Dictionary<uint, EntityController> Entities = new();
        private static readonly Dictionary<uint, PlayerController> Players = new();
        private static readonly Dictionary<uint, PortalController> Portals = new();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            // Clear cached connection data to ensure proper connection
            PlayerPrefs.DeleteAll();

            Instance = this;
            Application.targetFrameRate = 60;

            // In order to build a connection to SpacetimeDB we need to register
            // our callbacks and specify a SpacetimeDB server URI and module name.
            var builder = DbConnection.Builder()
                .OnConnect(HandleConnect)
                .OnConnectError(HandleConnectError)
                .OnDisconnect(HandleDisconnect)
                .WithUri(SpacetimeDbUrl)
                .WithModuleName(SpacetimeDbName);

            // If the user has a SpacetimeDB auth token stored in the Unity PlayerPrefs,
            // we can use it to authenticate the connection.
            if (AuthToken.Token != "")
            {
                builder = builder.WithToken(AuthToken.Token);
            }

            // Building the connection will establish a connection to the SpacetimeDB server.
            Connection = builder.Build();
        }

        public static bool IsConnected()
        {
            return Connection is { IsActive: true };
        }

        public void Disconnect()
        {
            Connection.Disconnect();
            Connection = null;
        }

        // Called when we connect to SpacetimeDB and receive our client identity
        private void HandleConnect(DbConnection connection, Identity identity, string token)
        {
            Debug.Log("Conncted");
            AuthToken.SaveToken(token);
            LocalIdentity = identity;


            Connection.Db.World.OnInsert += WorldOnInsert;
            Connection.Db.World.OnUpdate += WorldOnUpdate;

            Connection.Db.Terrain.OnDelete += OnTileRemoved;

            Connection.Db.Portal.OnInsert += PortalOnInsert;
            Connection.Db.Portal.OnUpdate += PortalOnUpdate;

            Connection.Db.Pill.OnInsert += PillOnInsert;
            Connection.Db.Pill.OnUpdate += PillOnUpdate;
            Connection.Db.Pill.OnDelete += PillOnDelete;

            Connection.Db.Projectile.OnInsert += ProjectileOnInsert;
            Connection.Db.Projectile.OnDelete += ProjectileOnDelete;

            Connection.Db.Entity.OnUpdate += EntityOnUpdate;
            Connection.Db.Entity.OnDelete += EntityOnDelete;

            Connection.Db.Player.OnInsert += PlayerOnInsert;
            Connection.Db.Player.OnDelete += PlayerOnDelete;


            OnConnected?.Invoke();

            // Request all tables
            Connection.SubscriptionBuilder()
                .OnApplied(HandleSubscriptionApplied)
                .SubscribeToAllTables();
        }

        #region Connection Handlers

        private static void HandleConnectError(Exception ex)
        {
            Debug.LogError($"Connection error: {ex}");
        }

        private static void HandleDisconnect(DbConnection connection, Exception ex)
        {
            Debug.Log("Disconnected.");
            if (ex != null)
            {
                Debug.LogException(ex);
            }
        }

        private static void HandleSubscriptionApplied(SubscriptionEventContext ctx)
        {
            Debug.Log("Subscription applied!");
            OnSubscriptionApplied?.Invoke();

            var seed = Guid.NewGuid().GetHashCode();
            Log.Debug($"Generating world with seed: {seed}");

            Connection.Reducers.GenerateTerrain(seed);

            RenderWorld(Connection.Db.World.Iter().FirstOrDefault());
            
            StartScreenHandler.Instance.Show();
            
        }

        #endregion

        #region World Handlers

        private void WorldOnInsert(EventContext ctx, World insertedValue)
        {
            Log.Debug("WorldOnInsert: World table inserted");
        }

        private void WorldOnUpdate(EventContext ctx, World oldWorld, World newWorld)
        {
            RenderWorld(newWorld);
        }

        private static void RenderWorld(World world)
        {
            if (world.IsGenerated)
            {
                Log.Debug("WorldOnUpdate: World table updated, generating ground...");
                TerrainHandler.Instance.Render();
            }
        }

        #endregion

        #region Terrain Handlers

        private static void OnTileRemoved(EventContext ctx, Terrain row)
        {
            TerrainHandler.Instance.OnTileRemoved(ctx, row);
        }

        #endregion
        
        #region Portal Handlers
        
        private static void PortalOnInsert(EventContext context, Portal insertedValue)
        {
            var portalController = PrefabSpawner.Instance.SpawnPortal(insertedValue);
            Portals.Add(insertedValue.Id, portalController);
        }
        
        private static void PortalOnUpdate(EventContext context, Portal oldRow, Portal newRow)
        {
            if (!Portals.TryGetValue(newRow.Id, out var portalController))
            {
                return;
            }

            portalController.OnPortalUpdated(newRow);
        }
        
        #endregion

        #region Pill Handlers

        private static void PillOnInsert(EventContext context, Pill insertedValue)
        {
            Log.Debug(
                $"PillOnInsert: Inserting pill for player {insertedValue.PlayerId} with entity ID {insertedValue.EntityId} position {insertedValue.Position}");
            var player = GetOrCreatePlayer(insertedValue.PlayerId);
            var entityController = PrefabSpawner.Instance.SpawnPill(insertedValue, player);
            Entities.Add(insertedValue.EntityId, entityController);
        }

        private static void PillOnUpdate(EventContext context, Pill oldPill, Pill newPill)
        {
            Log.Debug($"PillOnUpdate: Updating pill for player {newPill.PlayerId} with entity ID {newPill.EntityId}");
            if (!Entities.TryGetValue(newPill.EntityId, out var entityController))
            {
                return;
            }

            ((PillController)entityController).OnPillUpdated(newPill);
        }

        private static void PillOnDelete(EventContext context, Pill oldEntity)
        {
            var pillz = Connection.Db.Pill.PlayerId.Filter(oldEntity.PlayerId);
            if (pillz.Any() || !Players.ContainsKey(oldEntity.PlayerId))
                return;

            Log.Debug($"PillOnDelete: No pillz left for player {oldEntity.PlayerId}, removing player controller.");
            Players.Remove(oldEntity.PlayerId);
        }

        #endregion

        #region Projectile Handlers

        private static void ProjectileOnInsert(EventContext context, Projectile insertedValue)
        {
            var player = GetOrCreatePlayer(insertedValue.PlayerId);

            var entity = Connection.Db.Entity.Id.Find(insertedValue.EntityId);

            if (entity == null)
            {
                Log.Error($"[ProjectileOnInsert] No entity found for projectile {insertedValue.EntityId}");
                return;
            }

            var spawnPos = (Vector2)entity.Position;

            var entityController = player.Pill.GetWeapons().Shoot(insertedValue, player, spawnPos, insertedValue.Speed);
            Entities.Add(insertedValue.EntityId, entityController);
        }

        private void ProjectileOnDelete(EventContext context, Projectile projectile)
        {
            if (Entities.Remove(projectile.EntityId, out var entityController))
            {
                entityController.OnDelete(context);
            }
        }

        #endregion

        #region Entity Handlers

        private static void EntityOnUpdate(EventContext context, Entity oldEntity, Entity newEntity)
        {
            if (!Entities.TryGetValue(newEntity.Id, out var entityController))
            {
                return;
            }

            entityController.OnEntityUpdated(newEntity);
        }

        private static void EntityOnDelete(EventContext context, Entity oldEntity)
        {
            if (Entities.Remove(oldEntity.Id, out var entityController))
            {
                entityController.OnDelete(context);
            }
        }

        #endregion

        #region Player Handlers

        private static void PlayerOnInsert(EventContext context, Player insertedPlayer)
        {
            GetOrCreatePlayer(insertedPlayer.Id);
        }

        private static void PlayerOnDelete(EventContext context, Player deletedValue)
        {
            Log.Debug($"PlayerOnDelete");

            if (Players.Remove(deletedValue.Id, out var playerController))
            {
                Log.Debug(
                    $"PlayerOnDelete: Removing player controller for player ID: {deletedValue.Id} player count remaining: {Players.Count}");

                playerController.OnDelete(context);
            }
        }

        private static PlayerController GetOrCreatePlayer(uint playerId)
        {
            if (Players.TryGetValue(playerId, out var playerController))
                return playerController;

            Log.Debug("Creating new player controller for player ID: " + playerId);
            var player = Connection.Db.Player.Id.Find(playerId);
            playerController = PrefabSpawner.Instance.SpawnPlayer(player);
            Players.Add(playerId, playerController);

            return playerController;
        }

        #endregion
    }

// This is a workaround for the Unity IL2CPP compiler, which does not support the IsExternalInit type.
// It allows us to use init-only properties in our structs.
    internal static class IsExternalInit
    {
    }
}