using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using MihaZupan;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using nl_fhir;

namespace TelegramBotConsole
{
    class Program
    {
        static ITelegramBotClient teleBot;
        static NLModel nl;

        static void Main()
        {
            teleBot = new TelegramBotClient(AppInfo.TelegramToken);
            nl = new NLModel();
            
            //telegram
            try
            {
                var telegram = teleBot.GetMeAsync().Result;
                Console.WriteLine(
                  $"{telegram.FirstName} работает."
                );
                teleBot.OnMessage += Bot_OnMessage;
                teleBot.StartReceiving();
            }
            catch (Exception e) { Console.WriteLine("Ошибка при запуске Telegram бота: " + e.Message); }

            Thread.Sleep(int.MaxValue);
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine($"Получил сообщение в чате {e.Message.Chat.Id}.");
        }

    }
}
