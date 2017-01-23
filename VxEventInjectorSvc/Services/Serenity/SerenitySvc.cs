using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VxEventAgent;
using VxEventInjectorCommon;

namespace VxEventInjectorSvc.Services.Serenity
{
    class SerenitySvc : ISerenitySvc
    {
        private IUnityContainer _container;
        private SerenitySystem _serenitySystem;
        private List<SerenitySituation> _situations = new List<SerenitySituation>();

        public SerenitySvc(IUnityContainer container)
        {
            _container = container;
            _serenitySystem = _container.Resolve<SerenitySystem>();
        }

        public bool IsLoggedIn
        {
            get { return _serenitySystem.IsLoggedIn; }
        }

        public async Task<bool> LoginAsync(Credentials credentials)
        {
            await LogoutAsync();
            return await _serenitySystem.LoginAsync(credentials);
        }

        public async Task<bool> LogoutAsync()
        {
            if (_serenitySystem.IsLoggedIn)
            {
                await _serenitySystem.LogoutAsync();
                _situations.Clear();
            }
            return true;
        }

        public async Task<List<SerenitySituation>> GetSituations(bool useCache = true)
        {
            if (!_serenitySystem.IsLoggedIn) return new List<SerenitySituation>(); ;
            if (useCache && _situations.Count != 0)
                return _situations;

            _situations.Clear();
            var serSituations = await _serenitySystem.GetSituationsAsync();
            foreach (var serSituation in serSituations)
                _situations.Add(serSituation.ToSerenitySituation());
            return _situations;
        }

        public async Task<bool> InjectSituationAsync(INewSituation situation)
        {
            if (!_serenitySystem.IsLoggedIn) return false;

            bool success = await _serenitySystem.AddSituationAsync(situation);
            if (success && situation.Resources != null && situation.Resources.Count > 0)
            {
                var serSituation = (await (_serenitySystem.GetSituationsAsync(situation.Type, situation.SourceDeviceId)).ConfigureAwait(false)).FirstOrDefault();
                foreach (var resource in situation.Resources)
                {
                    if (!success)
                        break;

                    switch (resource.Key)
                    {
                        case ResourceType.DataSource:
                            {
                                var dataSource = await _serenitySystem.GetDataSource(resource.Value).ConfigureAwait(false);
                                var result = serSituation.Link(dataSource);
                                success = result == CPPCli.Results.Value.OK;
                                break;
                            }
                        case ResourceType.Device:
                            {
                                var device = await _serenitySystem.GetDevice(resource.Value).ConfigureAwait(false);
                                var result = serSituation.Link(device);
                                success = result == CPPCli.Results.Value.OK;
                                break;
                            }
                    }
                }
            }
            return success;
        }

        public async Task<bool> InjectEventAsync(INewEvent evt)
        {
            if (!_serenitySystem.IsLoggedIn) return false;

            bool success = await _serenitySystem.InjectEventAsync(evt).ConfigureAwait(false);
            return success;
        }
    }
}
