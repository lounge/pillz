using System;
using JetBrains.Annotations;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

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
        private static DbConnection Connection { get; set; }
        
        
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
                builder = builder.WithToken(AuthToken.Token); ;
            }

            // Building the connection will establish a connection to the SpacetimeDB
            // server.
            Connection = builder.Build();
        }
        
        // Called when we connect to SpacetimeDB and receive our client identity
        private void HandleConnect(DbConnection connection, Identity identity, string token)
        {
            Debug.Log("Conncted");
            AuthToken.SaveToken(token);
            LocalIdentity = identity;

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
            Debug.Log($"Disconnected.");
            if (ex != null)
            {
                Debug.LogException(ex);
            }
        }

        private static void HandleSubscriptionApplied(SubscriptionEventContext ctx)
        {
            Debug.Log("Subscription applied!");
            OnSubscriptionApplied?.Invoke();
        }
    }
}

namespace System.Runtime.CompilerServices
{
    // This is a workaround for the Unity IL2CPP compiler, which does not support the IsExternalInit type.
    // It allows us to use init-only properties in our structs.
    internal static class IsExternalInit { }
}

