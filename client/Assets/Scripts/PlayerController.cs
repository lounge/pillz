using System;
using SpacetimeDB.Types;
using UnityEngine;

namespace masks.client.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        private uint _playerId;
        private static PlayerController _local;
        public bool IsLocalPlayer => this == _local;
        public string Username => GameManager.Connection.Db.Player.Id.Find(_playerId)?.Name;
        
        
        public void Initialize(Player player)
        {
            _playerId = player.Id;
            if (player.Identity == GameManager.LocalIdentity)
            {
                _local = this;
            }
        }
        public void OnDelete(EventContext context)
        {
            Destroy(gameObject);
        }
        
    }
}