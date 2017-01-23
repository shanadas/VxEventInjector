using Pelco.Helpers;
using System.IO;
using System.Text;
using VxEventInjectorCommon;

namespace VxEventInjectorSvc.Services.Cache
{
    class CacheSvc : ICacheSvc
    {
        private const string _CachePasswd = "12345VxEventInjector67890";
        private static readonly object _CacheLock = new object();
        private static readonly string _CachePath = Path.Combine(Constants.InjectorDir, "cache.bin");
        private CacheDetails _cache = new CacheDetails();

        public CacheSvc()
        { }

        public CacheDetails Cache
        {
            get
            {
                CacheDetails cache = null;
                lock (_CacheLock)
                {
                    cache = new CacheDetails(_cache);
                }
                return cache;
            }
        }

        public CacheDetails Load()
        {
            CacheDetails cacheDetails = null;
            lock (_CacheLock)
            {
                if (File.Exists(_CachePath))
                {
                    var buffer = File.ReadAllBytes(_CachePath);
                    string json = Encoding.UTF8.GetString(buffer);
                    cacheDetails = Serialization.DeserializeJson<CacheDetails>(json);
                    cacheDetails.VxCredentials.Password = new Crypto(_CachePasswd).Decrypt<string>(cacheDetails.VxCredentials.Password);
                    _cache = cacheDetails;
                }
            }
            return cacheDetails;
        }

        public bool SaveCredentials(Credentials credentials)
        {
            bool success = true;
            lock (_CacheLock)
            {
                _cache.VxCredentials = credentials;
                success = Save();
            }
            return success;
        }

        public bool Add(string id)
        {
            bool success = true;
            lock (_CacheLock)
            {
                if (!_cache.AgentIds.Contains(id))
                {
                    _cache.AgentIds.Add(id);
                    success = Save();
                }
            }
            return success;
        }

        public bool Remove(string id)
        {
            bool success = true;
            lock (_CacheLock)
            {
                if (_cache.AgentIds.Contains(id))
                {
                    success = _cache.AgentIds.Remove(id);
                    success = success && Save();
                }
            }
            return success;
        }

        private bool Save()
        {
            lock (_CacheLock)
            {
                var cacheToSave = new CacheDetails(_cache);
                cacheToSave.VxCredentials.Password = new Crypto(_CachePasswd).Encrypt(cacheToSave.VxCredentials.Password);
                string json = Serialization.SerializeJson(cacheToSave);
                var buffer = Encoding.UTF8.GetBytes(json);
                File.WriteAllBytes(_CachePath, buffer);
            }
            return true;
        }
    }
}
