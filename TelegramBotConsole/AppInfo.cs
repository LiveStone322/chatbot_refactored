using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotConsole
{
    static class AppInfo
    {
        public static string Token { get; } = "824231491:AAG_0z_fj0u6aWQeR6ZSNJ0bghZkcfVvL-Y"; 
        public static string DbLogin { get; } = "postgres";
        public static string DbPassword { get; } = "123";
        public static string DbName { get; } = "health_bot";
        public static string DbHost { get; } = "localhost";
        public static int DbPort { get; } = 5432;
        public static string Socks5Host { get; set; } = "97.74.6.64";    //хардкод - наше всё
        public static int Socks5Port { get; set; } = 36671;
    }
}
