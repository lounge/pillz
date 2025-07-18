using UnityEngine;
using SpacetimeDB.Types;

namespace SpacetimeDB.Types
{
    public partial class DbVector2
    {
        public static implicit operator Vector2(DbVector2 v) => new Vector2(v.X, v.Y);
        public static implicit operator DbVector2(Vector2 v) => new() { X = v.x, Y = v.y };

    }
}