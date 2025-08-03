using SpacetimeDB.Types;
using UnityEngine;

namespace masks.client.Scripts
{
    public class PrefabManager : MonoBehaviour
    {
        private static PrefabManager _instance;
        public PlayerController playerPrefab;
        public MaskController maskPrefab;
        
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
            var entityController = Instantiate(_instance.maskPrefab, owner.transform);
            entityController.name = $"Mask_{owner.Username}";
            entityController.Spawn(mask, owner);
            
            owner.SetDefaults(entityController);
            
            return entityController;
        }
    }
}