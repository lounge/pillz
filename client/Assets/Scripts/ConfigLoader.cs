using System;
using System.IO;
using System.Linq;
using pillz.client.Scripts.Config;
using UnityEngine;

namespace pillz.client.Scripts
{
    public static class ConfigLoader
    {
        private const string ConfigName = "server.json";

        public static SpacetimeDbConfig LoadConfigCrossPlatform()
        {
            var streaming = GetStreamingConfigPath();
            var writable = GetWritableConfigPath();

            SpacetimeDbConfig overrides = null;

            if (!TryLoadConfigFromFile(streaming, out var defaults))
            {
                Debug.LogError($"[Config] Missing defaults: {streaming}. " +
                               "Ensure server.json is in the project's top-level StreamingAssets/ before building.");
                throw new FileNotFoundException($"Defaults not found: {streaming}");
            }

            if (!string.IsNullOrEmpty(writable))
            {
                try
                {
                    var dir = Path.GetDirectoryName(writable);
                    if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                    if (!File.Exists(writable) && File.Exists(streaming))
                    {
                        File.Copy(streaming, writable);
                        Debug.Log($"[Config] Seeded {writable} from StreamingAssets.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Config] Copy to writable failed: {e.Message}");
                }

                TryLoadConfigFromFile(writable, out overrides);
            }

            var merged = MergeConfigs(overrides, defaults);

            ValidateConfigOrThrow(merged);

            Debug.Log($"[Config] Using config: url={merged.url} db={merged.dbName}");
            return merged;
        }

        private static SpacetimeDbConfig MergeConfigs(SpacetimeDbConfig primary, SpacetimeDbConfig fallback)
        {
            if (primary == null) return fallback;

            return new SpacetimeDbConfig
            {
                url = !string.IsNullOrWhiteSpace(primary.url) ? primary.url : fallback?.url,
                dbName = !string.IsNullOrWhiteSpace(primary.dbName) ? primary.dbName : fallback?.dbName
            };
        }

        private static void ValidateConfigOrThrow(SpacetimeDbConfig cfg)
        {
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.url) || string.IsNullOrWhiteSpace(cfg.dbName))
                throw new Exception("[Config] Missing required fields: url, dbName");
        }

        private static bool TryLoadConfigFromFile(string path, out SpacetimeDbConfig cfg)
        {
            cfg = null;
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    if (!string.IsNullOrEmpty(path)) Debug.Log($"[Config] Not found: {path}");
                    return false;
                }

                var json = File.ReadAllText(path);
                var parsed = JsonUtility.FromJson<SpacetimeDbConfig>(json);

                cfg = parsed ?? throw new Exception("JSON parse returned null (check class uses public fields).");
                
                Debug.Log($"[Config] Loaded: {path}  url={cfg.url ?? "<null>"}  db={cfg.dbName ?? "<null>"}");
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
            TryEnsureDir(p);
            var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var bundle = string.IsNullOrWhiteSpace(Application.identifier) ? "Mulla" : Application.identifier;
            var alt = Path.Combine(home, "Library", "Application Support", bundle);
            if (TryEnsureDir(p))   return Path.Combine(p,   ConfigName);
            if (TryEnsureDir(alt)) return Path.Combine(alt, ConfigName);

            // Legacy Company/Product fallback
            var company =
 string.IsNullOrWhiteSpace(Application.companyName) ? "Mulla" : Sanitize(Application.companyName);
            var product =
 string.IsNullOrWhiteSpace(Application.productName) ? "Pillz" : Sanitize(Application.productName);
            var legacy = Path.Combine(home, "Library", "Application Support", company, product);
            if (TryEnsureDir(legacy)) return Path.Combine(legacy, ConfigName);

            Debug.LogWarning("[Config] No writable dir available; overrides disabled.");
            return null;
#else
            var p = Application.persistentDataPath;
            TryEnsureDir(p);
            return Path.Combine(p, ConfigName);
#endif
        }

        private static string GetStreamingConfigPath()
        {
            return Path.Combine(Application.streamingAssetsPath, ConfigName);
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