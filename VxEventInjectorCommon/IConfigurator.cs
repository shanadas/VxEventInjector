using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using VxEventAgent;

namespace VxEventInjectorCommon
{
    [ServiceContract(CallbackContract = typeof(IConfiguratorCallback))]
    [ServiceKnownType(typeof(BasicNewSituation))]
    public interface IConfigurator
    {
        [OperationContract]
        Task<bool> TestCredentialsAsync(Credentials credentials);

        [OperationContract]
        Task<List<string>> GetAllAgentIdsAsync();

        [OperationContract]
        Task<bool> RemoveAllAgentsAsync();

        [OperationContract]
        Task<bool> SetCredentialsAsync(Credentials credentials);

        [OperationContract]
        Task<bool> AddEventAgentAsync(string id);

        [OperationContract]
        Task<bool> AddSituationsAsync(string id, List<INewSituation> situations);

        [OperationContract]
        Task<bool> RunAllAgentsAsync();
    }
}
