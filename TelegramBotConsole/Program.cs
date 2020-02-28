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

namespace TelegramBotConsole
{
    class Program
    {
        static ITelegramBotClient telegramBot;

        static void Main()
        {
            telegramBot = new TelegramBotClient(AppInfo.TelegramToken, new HttpToSocks5Proxy("localhost", 8080));
            
            //telegram
            try
            {
                var telegram = telegramBot.GetMeAsync().Result;
                Console.WriteLine(
                  $"{telegram.FirstName} работает."
                );
                telegramBot.OnMessage += Bot_OnMessage;
                telegramBot.StartReceiving();
            }
            catch (Exception e) { Console.WriteLine("Ошибка при запуске Telegram бота: " + e.Message); }

            Thread.Sleep(int.MaxValue);
        }

        static void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine($"Получил сообщение в чате {e.Message.Chat.Id}.");
            foreach (PropertyInfo propertyInfo in e.Message.GetType().GetProperties())
            {
                Console.WriteLine(propertyInfo.Name + " : " + propertyInfo.GetValue(e.Message));
            }
        }

    }
}
