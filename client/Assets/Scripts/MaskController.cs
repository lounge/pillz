using SpacetimeDB.Types;
using UnityEngine;

namespace masks.client.Scripts
{
    public class MaskController : EntityController
    {
        public void Spawn(Mask mask, PlayerController owner)
        {
            base.Spawn(mask.EntityId, owner);
        }
    }
}