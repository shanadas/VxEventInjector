using VxEventInjector.Models;
using VxEventInjectorCommon;

namespace VxEventInjector.Extensions
{
    static class LoginExtensions
    {
        public static Credentials ToCredentials(this VxLoginModel loginModel)
        {
            var credentials = new Credentials()
            {
                IP = loginModel.IP,
                Port = loginModel.Port,
                SSL = loginModel.Port == 443,
                Username = loginModel.Username,
                Password = loginModel.Password
            };
            return credentials;
        }
    }
}
