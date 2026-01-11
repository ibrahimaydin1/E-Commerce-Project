// Basit cache helper
public static class CacheHelper
{
    private static readonly Dictionary<string, (object data, DateTime expiry)> _cache = new();
    
    public static T? Get<T>(string key)
    {
        if (_cache.ContainsKey(key) && _cache[key].expiry > DateTime.Now)
        {
            return (T?)_cache[key].data;
        }
        return default;
    }
    
    public static void Set(string key, object data, int minutes = 30)
    {
        _cache[key] = (data, DateTime.Now.AddMinutes(minutes));
    }
    
    public static void Clear(string key)
    {
        _cache.Remove(key);
    }
}
