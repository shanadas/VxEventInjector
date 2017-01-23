using System.ServiceModel;

namespace VxEventInjectorCommon
{
    public interface IConfiguratorCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnSituation(double percent, string message);
    }
}
