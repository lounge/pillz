using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace masks.client.Scripts
{
    public class PrefabManager : MonoBehaviour
    {
        private static PrefabManager _instance;
        public PlayerController playerPrefab;
        public MaskController maskPrefab;
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
        
        public static MaskController SpawnMask(Mask mask, PlayerController owner)
        {
            Log.Debug($"PrefabManager1: Spawning mask at position {mask.Position} with aim direction {mask.AimDir}.");
            
            var entityController = Instantiate(_instance.maskPrefab, owner.transform);
            entityController.name = $"Mask_{owner.Username}";
            
            Log.Debug($"PrefabManager2: Spawning mask at position {mask.Position} with aim direction {mask.AimDir}.");

            entityController.Spawn(mask, owner);
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