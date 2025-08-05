using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class PrefabManager : MonoBehaviour
    {
        private static PrefabManager _instance;
        public PlayerController playerPrefab;
        public PillController pillPrefab;
        public PortalController portalPrefab;
        
        private void Awake()
        {
            _instance = this;
        }

        public static PlayerController SpawnPlayer(Player player)
        {
            var playerController = Instantiate(_instance.playerPrefab);
            playerController.name = $"Player_{player.Username}";
            playerController.Initialize(player);
            return playerController;
        }
        
        public static PillController SpawnPill(Pill pill, PlayerController owner)
        {
            Log.Debug($"PrefabManager1: Spawning pill at position {pill.Position} with aim direction {pill.AimDir}.");
            
            var entityController = Instantiate(_instance.pillPrefab, owner.transform);
            entityController.name = $"Pill_{owner.Username}";

            entityController.Spawn(pill, owner);
            owner.SetDefaults(entityController);
            
            return entityController;
        }

        public static PortalController SpawnPortal(Portal portal)
        {
            var portalController = Instantiate(_instance.portalPrefab);
            portalController.name = $"Portal_{portal.Id}";
            
            portalController.Spawn(portal);
            
            Log.Debug($"Spawned portal at position {portal.Position} with connected portal ID {portal.ConnectedPortalId}.");

            return portalController;
        }
    }
}