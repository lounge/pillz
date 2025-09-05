using System;
using System.IO;
using System.Linq;
using pillz.client.Scripts.Config;
using UnityEngine;

namespace pillz.client.Scripts
{
    public static class ConfigLoader
    {
        private static SpacetimeDbConfig _config;

        private static readonly SpacetimeDbConfig DefaultConfig = new()
        {
            url = "http://localhost:3000",
            dbName = "pillz"
        };

        private const string FileName = "server.json";

        public static SpacetimeDbConfig LoadConfigCrossPlatform()
        {
            var streaming = ConfigLoader.GetStreamingConfigPath();
            var writable = ConfigLoader.GetWritableConfigPath();

#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
    if (Application.dataPath.Contains("AppTranslocation"))
        Debug.LogWarning($"[Config] App is translocated: {Application.dataPath}");
#endif

            Debug.Log($"[Config] streamingAssetsPath = {Application.streamingAssetsPath}");
            Debug.Log($"[Config] persistentDataPath = {Application.persistentDataPath}");
            Debug.Log($"[Config] writableConfigPath = {writable}");

            // Copy once if missing in writable location
            if (!string.IsNullOrEmpty(writable))
            {
                try
                {
                    if (!File.Exists(writable) && File.Exists(streaming))
                    {
                        File.Copy(streaming, writable);
                        Debug.Log($"[Config] Seeded {writable} from StreamingAssets.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Config] Copy failed: {e.Message}");
                }

                if (TryLoadConfigFromFile(writable, out _config)) return _config;
            }

            if (TryLoadConfigFromFile(streaming, out _config)) return _config;

            Debug.LogWarning("[Config] Using built-in defaults.");
            return DefaultConfig;
        }


        private static bool TryLoadConfigFromFile(string path, out SpacetimeDbConfig cfg)
        {
            cfg = null;
            try
            {
                if (!File.Exists(path))
                {
                    Debug.Log($"[Config] Not found: {path}");
                    return false;
                }

                var json = File.ReadAllText(path);
                var parsed = JsonUtility.FromJson<SpacetimeDbConfig>(json);
                if (parsed == null || string.IsNullOrWhiteSpace(parsed.url) || string.IsNullOrWhiteSpace(parsed.dbName))
                    throw new Exception("Missing required fields: url, dbName");

                cfg = parsed;
                Debug.Log($"[Config] Loaded: {path}  url={cfg.url}  db={cfg.dbName}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Config] Failed to load {path}: {ex.Message}");
                return false;
            }
        }


        private static string GetWritableConfigPath()
        {
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        var p = Application.persistentDataPath;
        // Belt-and-suspenders: make a second path based on the bundle identifier
        // ~/Library/Application Support/<bundle id>/
        var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        var bundle = string.IsNullOrWhiteSpace(Application.identifier) ? "com.company.product" : Application.identifier;
        var alt = Path.Combine(home, "Library", "Application Support", bundle);

        // Prefer Unity’s path if we can make it work
        if (TryEnsureDir(p)) return Path.Combine(p, FileName);
        // Fall back to the identifier-based path
        if (TryEnsureDir(alt)) return Path.Combine(alt, FileName);

        // Last-ditch: ApplicationSupport/<Company>/<Product>
        var company =
 string.IsNullOrWhiteSpace(Application.companyName) ? "Company" : Sanitize(Application.companyName);
        var product =
 string.IsNullOrWhiteSpace(Application.productName) ? "Product" : Sanitize(Application.productName);
        var legacy = Path.Combine(home, "Library", "Application Support", company, product);
        if (TryEnsureDir(legacy)) return Path.Combine(legacy, FileName);

        // If everything failed, at least don’t crash
        Debug.LogWarning("[Config] Could not create any writable dir; using in-memory defaults.");
        return null;
#else
            // Windows/Linux: persistentDataPath is fine.
            var p = Application.persistentDataPath;
            TryEnsureDir(p); // best-effort
            return Path.Combine(p, FileName);
#endif
        }

        private static string GetStreamingConfigPath()
        {
            return Path.Combine(Application.streamingAssetsPath, FileName);
        }

        private static bool TryEnsureDir(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir)) return false;
            try
            {
                Directory.CreateDirectory(dir);
                return Directory.Exists(dir);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Config] CreateDirectory failed for {dir}: {e.Message}");
                return false;
            }
        }

        private static string Sanitize(string s)
        {
            s = Path.GetInvalidFileNameChars().Aggregate(s, (current, c) => current.Replace(c.ToString(), "_"));
            s = s.Replace(":", "_").Trim();
            return s;
        }
    }
}