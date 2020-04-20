using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZulipAPI;

namespace WebApp
{
    public class Zulip
    {
        private static string _serverURL;
        public static string ServerURL { get; } = "zulipapp.my.to";

        public static string ApiKey => client?.APIKey;

        private static string _password;
        public static string Password
        {
            get { return _password; }
            set { _password = value; Connected = false; }
        }
        private static bool _connected;
        public static bool Connected
        {
            get { return _connected; }
            set { _connected = value; }
        }

        public static ZulipClient client;

        public static async Task GetZulipClient()
        {
            if (!Connected && !string.IsNullOrEmpty(Password))
            {
                ZulipServer ZuSrv = new ZulipServer(ServerURL);
                client = await ZuSrv.LoginAsync(UserEmail, Password);
                Connected = client != null;
            }
        }

        public static void GetZulipClient(string ZulipRCPath)
        {
            if (!Connected)
            {
                client = ZulipServer.Login(ZulipRCPath);
                Connected = client != null && !string.IsNullOrEmpty(ApiKey);
            }
        }

        public static async Task GetZulipClient(string userEmail, string password)
        {
            if (!Connected)
            {
                ZulipServer ZuSrv = new ZulipServer(ServerURL);
                client = await ZuSrv.LoginAsync(UserEmail, Password);
                Connected = client != null && !string.IsNullOrEmpty(ApiKey);
            }
        }
    }
}
