using System.Collections.Generic;
using System.Threading.Tasks;
using VxEventAgent;
using VxEventInjectorCommon;

namespace VxEventInjectorSvc.Services.Serenity
{
    interface ISerenitySvc
    {
        bool IsLoggedIn { get; }
        Task<bool> LoginAsync(Credentials credentials);
        Task<bool> LogoutAsync();
        Task<List<SerenitySituation>> GetSituations(bool useCache = true);
        Task<bool> InjectSituationAsync(INewSituation situation);
        Task<bool> InjectEventAsync(INewEvent evt);
    }
}
