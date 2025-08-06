using System;
using System.Linq;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
using Terrain = SpacetimeDB.Types.Terrain;

namespace pillz.client.Scripts
{
    public class TerrainManager : MonoBehaviour
    {
        public static TerrainManager Instance { get; private set; }
        
        [Header("Clamp Settings")] [SerializeField]
        private Collider2D deathZone;

        public Tilemap tilemap;
        public TileBase terrainTile;
        public GameObject crumblePrefab;
        
        [NonSerialized] public float MinY;
        [NonSerialized] public float MinX;
        [NonSerialized] public float MaxX;


        private void Awake()
        {
            Instance = this;

            if (!tilemap)
            {
                tilemap = GetComponent<Tilemap>();
            }
            
            if (deathZone)
            {
                var bounds = deathZone.bounds;
                MinY = bounds.max.y; 
                MinX = bounds.min.x;
                MaxX = bounds.max.x;
                Log.Debug("TerrainManager: Death zone bounds set to MinY: " + MinY + ", MinX: " + MinX + ", MaxX: " + MaxX);
            }
        }

        public void Render()
        {
            Log.Debug("TerrainManager: Generating terrain...");
            foreach (var tile in GameManager.Connection.Db.Terrain.Iter())
            {
                OnTerrainAdded(null, tile);
            }
            
            // TODO: ONLY FOR TESTING
            // foreach (var portal in GameManager.Connection.Db.Portal.Iter())
            // {
            //     Log.Debug("TerrainManager: Adding portal location at position " + new Vector3Int((int)portal.Position.X, (int)portal.Position.Y, 0));
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
            Instantiate(crumblePrefab, new Vector3(tile.Position.X, tile.Position.Y), Quaternion.identity);
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