using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    public struct PillIntent
    {
        public Vector2 Move;
        public bool JumpHeld;
        public bool ToggleJetpack;
        public WeaponType SelectWeapon;
    }
}