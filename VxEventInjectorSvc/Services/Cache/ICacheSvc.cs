using VxEventInjectorCommon;

namespace VxEventInjectorSvc.Services.Cache
{
    interface ICacheSvc
    {
        CacheDetails Cache { get; }
        CacheDetails Load();
        bool SaveCredentials(Credentials credentials);
        bool Add(string id);
        bool Remove(string id);
    }
}
