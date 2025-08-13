using System;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        private static PlayerController _local;
        public bool IsLocalPlayer => this == _local;
        public string Username => GameInit.Connection.Db.Player.Id.Find(PlayerId)?.Username;
        [field: SerializeField] public GameObject GameHud { get; private set; }
        
        [NonSerialized] 
        public PillController Pill;
        
        [NonSerialized]
        public uint PlayerId;

        public void Spawn(Player player)
        {
            PlayerId = player.Id;
            if (player.Identity == GameInit.LocalIdentity)
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