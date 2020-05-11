using System;
using System.Collections.Generic;
using System.Text;

namespace WebApp
{
    static class AppInfo
    {
        public static string TelegramToken { get; } = "824231491:AAG_0z_fj0u6aWQeR6ZSNJ0bghZkcfVvL-Y";
        public static string ViberToken { get; } = "4ab1909dba27d4f9-5979534a15d57be4-e416d3c08305903e";
        public static string IcqToken { get; } = "001.3482970284.2551975855:752724779";
        public static string DbLogin { get; } = "doadmin";
        public static string DbPassword { get; } = "dcwxq8x5sql288n7";
        public static string DbName { get; } = "healthbot";
        public static string DbHost { get; } = "db-postgresql-fra1-21567-do-user-7233904-0.a.db.ondigitalocean.com";
        public static string DbPort { get; } = "25060";
        public static string Socks5Host { get; set; } = "127.0.0.1";    //хардкод - наше всё
        public static int Socks5Port { get; set; } = 8080;
        public static bool isDebugging { get; set; } = true; 
        public static string ZulipPass { get; set; } = "LlpyVNXLgtZb5CXHow9EL7fzKl74o4fv";   
        public static string ZulipServerURL { get; set; } = "https://zulipapp.my.to/";
        public static string ZulipEmail { get; set; } = "live.stone.anton@yandex.ru";
    }
}
