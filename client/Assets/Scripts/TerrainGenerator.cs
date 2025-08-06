using System.Linq;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
using UnityEngine.Tilemaps;
using Terrain = SpacetimeDB.Types.Terrain;

namespace pillz.client.Scripts
{
    public class TerrainGenerator : MonoBehaviour
    {
        public static TerrainGenerator Instance { get; private set; }

        public Tilemap tilemap;
        public TileBase terrainTile;


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
            Log.Debug("TerrainGenerator: Generating terrain...");
            foreach (var tile in GameManager.Connection.Db.Terrain.Iter())
            {
                // Log.Debug("TerrainGenerator: Adding tile at position " + new Vector3Int(tile.X, tile.Y, 0));

                OnTerrainAdded(null, tile);
            }
            
            // TODO: ONLY FOR TESTING
            // foreach (var portal in GameManager.Connection.Db.Portal.Iter())
            // {
            //     Log.Debug("TerrainGenerator: Adding portal location at position " + new Vector3Int((int)portal.Position.X, (int)portal.Position.Y, 0));
            //     OnPortalLocAdded(null, portal);
            // }
        }

        private void OnTerrainAdded(EventContext ctx, Terrain tile)
        {
            var pos = new Vector3Int((int)tile.Position.X, (int)tile.Position.Y, 0);
            tilemap.SetTile(pos, terrainTile);

            // TODO: ONLY FOR TESTING
            // if (tile.IsSpawnable)
            // {
            //     tilemap.SetColor(pos, new Color(34f, 0f, 0f, 1f)); // Red
            // }
            // else
            // {
                tilemap.SetColor(pos, new Color32(8, 255, 177, 255)); // teal-green
            // }
        }

        public void OnPortalLocAdded(EventContext ctx, Portal portal)
        {
            var pos = new Vector3Int((int)portal.Position.X, (int)portal.Position.Y, 0);
            tilemap.SetTile(pos, terrainTile);
            tilemap.SetColor(pos, new Color(255, 0f, 0, 1f)); // red
        }

        public void OnTileRemoved(EventContext ctx, Terrain tile)
        {
            var pos = new Vector3Int((int)tile.Position.X, (int)tile.Position.Y, 0);
            tilemap.SetTile(pos, null);
        }


        public DbVector2 GetRandomSpawnPosition()
        {
            // TODO: ONLY FOR TESTING
            // var spawnLocations = GameManager.Connection.Db.SpawnLocation.Iter().FirstOrDefault(); //ToList();
            // return spawnLocations!.Position;

            var spawnLocations = GameManager.Connection.Db.Terrain.Iter().Where(x => x.IsSpawnable).ToList();
            if (spawnLocations.Count == 0)
            {
                throw new System.Exception("No spawn locations available.");
            }

            int index = Random.Range(0, spawnLocations.Count);
            return spawnLocations[index].Position;
        }
    }
}