using System;
using SpacetimeDB.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace pillz.client.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        private static PlayerController _local;
        public bool IsLocalPlayer => this == _local;
        public string Username => Game.Connection.Db.Player.Id.Find(PlayerId)?.Username;
        public Player Player => Game.Connection.Db.Player.Id.Find(PlayerId);
        
        [field: SerializeField] public GameObject GameHud { get; private set; }
        
        [NonSerialized] 
        public PillController Pill;
        
        [NonSerialized]                 
        public uint PlayerId;

        public Stats Stats { get; } = new();

        public void Spawn(Player player)
        {
            
            PlayerId = player.Id;
            if (player.Identity == Game.LocalIdentity)
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
        
        public void SetStats()
        {
            Stats.Username = Username;
            Stats.Deaths = Player.Deaths;
            Stats.Frags = Player.Frags;
            Stats.Dmg = Player.Dmg;
        }
    }
}