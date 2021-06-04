using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotConsole
{
    static class AppInfo
    {
        public static string TelegramToken { get; } = "824231491:AAHRnxMI1vHePjHXw3-TwhAwqyZzoe-M8qw";
        public static string ViberToken { get; } = "4ab1909dba27d4f9-5979534a15d57be4-e416d3c08305903e";
        public static string DbLogin { get; } = "postgres";
        public static string DbPassword { get; } = "123";
        public static string DbName { get; } = "chatbot";
        public static string DbHost { get; } = "healthbot.ckuwobxeqhcr.us-east-1.rds.amazonaws.com";
        public static int DbPort { get; } = 5432;
        public static string Socks5Host { get; set; } = "127.0.0.1";    //хардкод - наше всё
        public static int Socks5Port { get; set; } = 8080;
        public static string ConnString { get; set; } = $"Persist Security Info=True;Password{DbPassword}Username={DbLogin};Database={DbName};Host=localhost";
    }
}
