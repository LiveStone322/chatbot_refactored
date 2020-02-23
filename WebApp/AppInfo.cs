using System;
using System.Collections.Generic;
using System.Text;

namespace WebApp
{
    static class AppInfo
    {
        public static string TelegramToken { get; } = "824231491:AAG_0z_fj0u6aWQeR6ZSNJ0bghZkcfVvL-Y";
        public static string ViberToken { get; } = "4ab1909dba27d4f9-5979534a15d57be4-e416d3c08305903e";
        public static string DbLogin { get; } = "postgres";
        public static string DbPassword { get; } = "123123123";
        public static string DbName { get; } = "health_bot";
        public static string DbHost { get; } = "healthbot.ckuwobxeqhcr.us-east-1.rds.amazonaws.com";
        public static int DbPort { get; } = 5432;
        public static string Socks5Host { get; set; } = "97.74.6.64";    //хардкод - наше всё
        public static int Socks5Port { get; set; } = 36671;
    }
}
