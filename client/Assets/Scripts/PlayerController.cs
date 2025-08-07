using System;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        private static PlayerController _local;
        public bool IsLocalPlayer => this == _local;
        public string Username => GameManager.Connection.Db.Player.Id.Find(PlayerId)?.Username;

        [SerializeField] private GameObject gameHud;
        
        [NonSerialized] 
        public PillController Pill;
        
        [NonSerialized]
        public uint PlayerId;

        public void Init(Player player)
        {
            PlayerId = player.Id;
            if (player.Identity == GameManager.LocalIdentity)
            {
                _local = this;
            }
            
        }

        public GameObject GetHud()
        {
            return gameHud;
        }

        public void OnDelete(EventContext context)
        {
            Destroy(gameObject);
        }

        public void SetDefaults(PillController entityController)
        {
            name = $"Player_{Username}";
            Pill = entityController;
        }
    }
}