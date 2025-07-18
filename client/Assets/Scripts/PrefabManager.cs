using SpacetimeDB.Types;
using UnityEngine;

namespace masks.client.Scripts
{
    public class PrefabManager : MonoBehaviour
    {
        private static PrefabManager _instance;
        public PlayerController playerPrefab;

        private void Awake()
        {
            _instance = this;
        }

        public static PlayerController SpawnPlayer(Player player)
        {
            var playerController = Instantiate(_instance.playerPrefab);
            playerController.name = $"Player_{player.Id}";
            playerController.Initialize(player);
            return playerController;
        }
    }
}