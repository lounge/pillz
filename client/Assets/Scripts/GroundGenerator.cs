using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace masks.client.Scripts
{
    public class GroundGenerator : MonoBehaviour
    {
        public static GroundGenerator Instance { get; private set; }

        public Tilemap tilemap;
        public TileBase groundTile;


        private void Awake()
        {
            Instance = this;
            
            if (!tilemap)
            {
                tilemap = GetComponent<Tilemap>();
            }
        }

        public void Render()
        {
            Log.Debug("GroundGenerator: Generating ground...");
            foreach (var tile in GameManager.Connection.Db.Ground.Iter())
            {
                // Log.Debug("GroundGenerator: Adding tile at position " + new Vector3Int(tile.X, tile.Y, 0));
                OnTileAdded(null, tile);
            }
        }
        
        public void OnTileAdded(EventContext ctx, Ground tile)
        {
            var pos = new Vector3Int(tile.X, tile.Y, 0);
            tilemap.SetTile(pos, groundTile);
        }

        public void OnTileRemoved(EventContext ctx, Ground tile)
        {
            var pos = new Vector3Int(tile.X, tile.Y, 0);
            tilemap.SetTile(pos, null);
        }
    }
}