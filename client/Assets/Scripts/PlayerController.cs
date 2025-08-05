using System;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        [NonSerialized]
        public uint PlayerId;
        
        private static PlayerController _local;
        public bool IsLocalPlayer => this == _local;
        
        public string Username => GameManager.Connection.Db.Player.Id.Find(PlayerId)?.Username;


        [NonSerialized] 
        public PillController Pill;


        public void Initialize(Player player)
        {
            PlayerId = player.Id;
            if (player.Identity == GameManager.LocalIdentity)
            {
                _local = this;
            }
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