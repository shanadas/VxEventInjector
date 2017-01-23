using System;

namespace VxEventInjectorCommon
{
    [Serializable]
    public class Credentials
    {
        public string IP { get; set; }
        public ushort Port { get; set; }
        public bool SSL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public Credentials()
        { }

        public Credentials(Credentials other)
            : this()
        {
            if (other != null)
            {
                IP = other.IP;
                Port = other.Port;
                SSL = other.SSL;
                Username = other.Username;
                Password = other.Password;
            }
        }

        public override bool Equals(object obj)
        {
            Credentials cred = obj as Credentials;
            if(obj == null || cred == null) return false;

            bool success = true;
            success = success && IP.Equals(cred.IP);
            success = success && Port.Equals(cred.Port);
            success = success && SSL.Equals(cred.SSL);
            success = success && Username.Equals(cred.Username);
            success = success && Password.Equals(cred.Password);
            return success;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + IP.GetHashCode();
            hash = (hash * 7) + Port.GetHashCode();
            hash = (hash * 7) + SSL.GetHashCode();
            hash = (hash * 7) + Username.GetHashCode();
            hash = (hash * 7) + Password.GetHashCode();
            return hash;
        }
    }
}
