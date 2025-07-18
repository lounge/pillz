using SpacetimeDB.Types;
using UnityEngine;

namespace masks.client.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        protected uint PlayerId;
        
        protected static PlayerController Local { get; private set; }
        protected bool IsLocalPlayer => this == Local;
        
        protected string Username => GameManager.Connection.Db.Player.Id.Find(PlayerId)?.Name;
        
        
        public void Initialize(Player player)
        {
            PlayerId = player.Id;
            if (player.Identity == GameManager.LocalIdentity)
            {
                Local = this;
            }
        }
        public virtual void OnDelete(EventContext context)
        {
            Destroy(gameObject);
        }
        
    }
}