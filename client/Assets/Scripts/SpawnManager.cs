using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class SpawnManager : MonoBehaviour
    {
        public static SpawnManager Instance;
        
        [SerializeField] private PlayerController playerPrefab;
        [SerializeField] private PillController pillPrefab;
        [SerializeField] private PortalController portalPrefab;
        
        private void Awake()
        {
            Instance = this;
        }

        public PlayerController SpawnPlayer(Player player)
        {
            var playerController = Instantiate(Instance.playerPrefab);
            playerController.name = $"Player_{player.Username}";
            playerController.Init(player);
            return playerController;
        }
        
        public PillController SpawnPill(Pill pill, PlayerController owner)
        {
            Log.Debug($"PrefabManager1: Spawning pill at position {pill.Position} with aim direction {pill.AimDir}.");
            
            var entityController = Instantiate(Instance.pillPrefab, owner.transform);
            entityController.name = $"Pill_{owner.Username}";

            entityController.Spawn(pill, owner);
            owner.SetDefaults(entityController);
            
            return entityController;
        }

        public PortalController SpawnPortal(Portal portal)
        {
            var portalController = Instantiate(Instance.portalPrefab);
            portalController.name = $"Portal_{portal.Id}";
            
            portalController.Spawn(portal);
            
            Log.Debug($"Spawned portal at position {portal.Position} with connected portal ID {portal.ConnectedPortalId}.");

            return portalController;
        }
    }
}