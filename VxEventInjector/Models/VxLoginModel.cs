namespace VxEventInjector.Models
{
    class VxLoginModel
    {
        public string IP { get; set; }
        public ushort Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public VxLoginModel()
        {
            Port = 443;
        }
    }
}
