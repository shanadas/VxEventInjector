using Pelco.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VxEventAgent;
using VxEventInjectorCommon;

namespace VxEventInjectorSvc.Services.Serenity
{
    class SerenitySystem
    {
        private const string _ClientId = "6BA58C40-79CA-4A5F-8547-8500FA7CEBE3";
        private Credentials _currentCred;
        private ILogger _logger;
        private CPPCli.VXSystem _serSystem;

        public SerenitySystem(ILogger logger)
        {
            _logger = logger;
            var result = CPPCli.VxGlobal.InitializeSdk("62065CD559F8240984A4CF4F1398333EA891BD466561FF11023BCAFA8D890DBF");
            if (result != CPPCli.Results.Value.OK)
                _logger.LogThenThrow(new Exception($"Error initializing the SDK - {result}"));
        }

        public bool IsLoggedIn
        {
            get { return _serSystem != null && _currentCred != null; }
        }

        public Task<bool> LogoutAsync()
        {
            if (_serSystem != null)
            {
                _serSystem.Dispose();
                _serSystem = null;
            }
            _currentCred = null;
            return Task.FromResult(true);
        }

        public async Task<bool> LoginAsync(Credentials credentials)
        {
            bool success = false;
            await LogoutAsync();

            _currentCred = credentials;
            var users = new List<CPPCli.User>();
            try
            {
                _serSystem = new CPPCli.VXSystem(credentials.IP);
                CPPCli.Results.Value result = CPPCli.Results.Value.UnknownError;
                await Task.Run(() => result = _serSystem.Login(credentials.Username, credentials.Password)).ConfigureAwait(false);
                if (result == CPPCli.Results.Value.OK)
                {
                    await Task.Run(() => users = _serSystem.GetUsers()).ConfigureAwait(false);
                    success = users.Any(user => user.Name == credentials.Username);
                }
            }
            catch (Exception e)
            {
                _logger.Log(e);
            }
            return success;
        }

        public async Task<CPPCli.Device> GetDevice(string deviceId = null)
        {
            if (!IsLoggedIn) return null;

            CPPCli.Device device = null;
            try
            {
                List<CPPCli.Device> devices = null;
                await Task.Run(() => devices = _serSystem.GetDevices(deviceId)).ConfigureAwait(false);
                device = devices.FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.Log(e);
            }
            return device;
        }

        public async Task<CPPCli.DataSource> GetDataSource(string dataSourceId = null)
        {
            if (!IsLoggedIn) return null;

            CPPCli.DataSource dataSource = null;
            try
            {
                List<CPPCli.DataSource> dataSources = null;
                await Task.Run(() => dataSources = _serSystem.GetDataSources(dataSourceId)).ConfigureAwait(false);
                dataSource = dataSources.FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.Log(e);
            }
            return dataSource;
        }

        public async Task<bool> AddSituationAsync(INewSituation situation)
        {
            if (!IsLoggedIn) return false;

            bool success = false;
            try
            {
                CPPCli.Results.Value result = CPPCli.Results.Value.UnknownError;
                await Task.Run(() => result = _serSystem.AddSituation(situation.ToSerNewSituation())).ConfigureAwait(false);
                success = result == CPPCli.Results.Value.OK;
            }
            catch (Exception e)
            {
                _logger.Log(e);
            }
            return success;
        }

        public async Task<List<CPPCli.Situation>> GetSituationsAsync(string situationType = null, string sourceDeviceId = null)
        {
            List<CPPCli.Situation> serSituations = null;
            try
            {
                await Task.Run(() => serSituations = _serSystem.GetSituations(situationType, sourceDeviceId)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.Log(e);
            }
            serSituations = serSituations ?? new List<CPPCli.Situation>();
            return serSituations;
        }

        public async Task<bool> InjectEventAsync(INewEvent evt)
        {
            if (!IsLoggedIn) return false;

            bool success = false;
            try
            {
                CPPCli.Results.Value result = CPPCli.Results.Value.UnknownError;
                await Task.Run(() => result = _serSystem.InjectEvent(evt.ToSerNewEvent())).ConfigureAwait(false);
                success = result == CPPCli.Results.Value.OK;
            }
            catch (Exception e)
            {
                _logger.Log(e);
            }
            return success;
        }
    }
}
