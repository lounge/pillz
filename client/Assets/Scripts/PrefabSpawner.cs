using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class PrefabSpawner : MonoBehaviour
    {
        [SerializeField] private PlayerController playerPrefab;
        [SerializeField] private PillController pillPrefab;
        [SerializeField] private PortalController portalPrefab;
        [SerializeField] private AmmoController ammoPrefab;

        public PlayerController SpawnPlayer(Player player)
        {
            var playerController = Instantiate(playerPrefab);
            playerController.name = $"Player_{player.Username}";
            playerController.Spawn(player);
            return playerController;
        }

        public PillController SpawnPill(Pill pill, PlayerController owner)
        {
            Log.Debug($"PrefabManager1: Spawning pill at position {pill.Position} with aim direction {pill.AimDir}.");

            var entityController = Instantiate(pillPrefab, owner.transform);
            entityController.name = $"Pill_{owner.Username}";

            entityController.Spawn(pill, owner);
            owner.SetDefaults(entityController);

            return entityController;
        }

        public PortalController SpawnPortal(Portal portal)
        {
            var portalController = Instantiate(portalPrefab);
            portalController.name = $"Portal_{portal.Id}";

            portalController.Spawn(portal);

            Log.Debug(
                $"Spawned portal at position {portal.Position} with connected portal ID {portal.ConnectedPortalId}.");

            return portalController;
        }
        
        public AmmoController SpawnAmmo(Ammo ammo)
        {
            var ammoController = Instantiate(ammoPrefab);
            ammoController.name = $"Ammo_{ammo.AmmoType}_{ammo.Id}";

            ammoController.Spawn(ammo);

            Log.Debug($"Spawned ammo with EntityId {ammo.Id} and ammoType {ammo.AmmoType}.");

            return ammoController;
        }
    }
}