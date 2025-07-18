using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
using DbConnection = SpacetimeDB.Types.DbConnection;

namespace masks.client.Scripts
{
    public class GameManager : MonoBehaviour
    {
        private const string SpacetimeDbUrl = "http://localhost:3000";
        private const string SpacetimeDbName = "masks";

        [UsedImplicitly] private static event Action OnConnected;
        [UsedImplicitly] private static event Action OnSubscriptionApplied;

        public static GameManager Instance { get; private set; }
        public static Identity LocalIdentity { get; private set; }
        public static DbConnection Connection { get; set; }

        private static readonly Dictionary<uint, EntityController> Entities = new();
        private static readonly Dictionary<uint, PlayerController> Players = new();


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

            
            Connection.Db.Mask.OnInsert += MaskOnInsert;

            
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

            ctx.Reducers.EnterGame("MULLA_JAFFAR");
        }
        
        
        
        
        private static void MaskOnInsert(EventContext context, Mask insertedValue)
        {
            var player = GetOrCreatePlayer(insertedValue.PlayerId);
            var entityController = PrefabManager.SpawnMask(insertedValue, player);
            Entities.Add(insertedValue.EntityId, entityController);
        }
        
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


        private static void PlayerOnInsert(EventContext context, Player insertedPlayer)
        {
            GetOrCreatePlayer(insertedPlayer.Id);
        }

        private static void PlayerOnDelete(EventContext context, Player deletedValue)
        {
            if (Players.Remove(deletedValue.Id, out var playerController))
            {
                playerController.OnDelete(context);
                // Destroy(playerController.gameObject);
            }
        }
        
        private static PlayerController GetOrCreatePlayer(uint playerId)
        {
            if (!Players.TryGetValue(playerId, out var playerController))
            {
                var player = Connection.Db.Player.Id.Find(playerId);
                playerController = PrefabManager.SpawnPlayer(player);
                Players.Add(playerId, playerController);
            }

            return playerController;
        }
        
        
        
    }
}

namespace System.Runtime.CompilerServices
{
    // This is a workaround for the Unity IL2CPP compiler, which does not support the IsExternalInit type.
    // It allows us to use init-only properties in our structs.
    internal static class IsExternalInit
    {
    }
}
