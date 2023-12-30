using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Console.CachingService
{
    

    public class FileCache
    {
        private readonly string _filePath;
        private Dictionary<string, string> _cache;

        public FileCache(string filePath = "cache.cache")
        {
            _filePath = $"{Directory.GetCurrentDirectory()}\\{filePath}";
            LoadCache();
        }

        private void LoadCache()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _cache = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            else
            {
                _cache = new Dictionary<string, string>();
            }
        }

        public void AddOrUpdate(string key, string value)
        {
            _cache[key] = value;
            SaveCache();
        }

        public bool TryGetValue(string key, out string value)
        {
            return _cache.TryGetValue(key, out value);
        }

        private void SaveCache()
        {
            var json = JsonSerializer.Serialize(_cache);
            File.WriteAllText(_filePath, json);
        }
    }

}
