using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using MihaZupan;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Viber.Bot;

namespace TelegramBotConsole
{
    class Program
    {
        static ITelegramBotClient telegramBot;

        static void Main()
        {
            telegramBot = new TelegramBotClient(AppInfo.TelegramToken, new HttpToSocks5Proxy(AppInfo.Socks5Host, AppInfo.Socks5Port));
            
            //telegram
            try
            {
                var telegram = telegramBot.GetMeAsync().Result;
                Console.WriteLine(
                  $"{telegram.FirstName} работает."
                );
                telegramBot.OnMessage += Bot_OnMessage;
                telegramBot.StartReceiving();

                //подключение к бд
                using (var con = new Npgsql.NpgsqlConnection(new Npgsql.NpgsqlConnectionStringBuilder
                {
                    Host = AppInfo.DbHost,
                    Port = AppInfo.DbPort,
                    Username = AppInfo.DbLogin,
                    Password = AppInfo.DbPassword,
                    Database = AppInfo.DbName
                }.ConnectionString)
                )
                {
                    con.Open();
                }
            }
            catch (Exception e) { Console.WriteLine("Ошибка при запуске Telegram бота: " + e.Message); }

            Thread.Sleep(int.MaxValue);
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            DialogueFrame df;
            Console.WriteLine($"Получил сообщение в чате {e.Message.Chat.Id}.");

            using (var ctx = new HealthBotContext())
            {
                var tlgrmUser = e.Message.From;
                var dbUser = ctx.Users.Where(t => t.login == tlgrmUser.Username).FirstOrDefault();

                if (dbUser == null) //если пользователя нет
                {
                    dbUser = new users()
                    {
                        id = tlgrmUser.Id.ToString(),
                        login = tlgrmUser.Username,
                        id_source = 1,
                        fio = tlgrmUser.FirstName + " " + tlgrmUser.LastName,
                    };
                    ctx.Users.Add(dbUser);
                }

                //обработка сообщения (Dialogue state tracker)
                df = DialogueFrame.GetDialogueFrame(e, ctx, dbUser);

                //внутренняя работа
                switch (df.Activity)
                {
                    case DialogueFrame.EnumActivity.Answer:
                        await ctx.Questions_answers.AddAsync(new questions_answers
                        {
                            id_user = dbUser.id,
                            id_question = (int)df.Tag,
                            value = df.Entity
                        });
                        break;
                    case DialogueFrame.EnumActivity.LoadFile: 
                        var path = Path.GetFullPath(@"..\..\");
                        var name = e.Message.Photo[e.Message.Photo.Length - 1].FileId;
                        DownloadFile(name, path + name);
                        ctx.Files.Add(new files
                        {
                            content_hash = name,
                            directory = "test",
                            id_user = dbUser.id,
                            file_name = name,
                            file_format = "jpg",
                            id_source = 1
                        });
                        break;
                    case DialogueFrame.EnumActivity.ReadMyBiomarkers:
                        dbUser.id_last_question = null;
                        break;
                    case DialogueFrame.EnumActivity.ConversationStart: break;
                    case DialogueFrame.EnumActivity.Unknown: break;
                }
                await ctx.SaveChangesAsync();

                //обработка следующего сообщения (Dialogue state manager)
                DialogueFrame.SendNextMessage(df, ctx, dbUser, e.Message.Chat, telegramBot);
            }
        }

        private static async void DownloadFile(string fileId, string path)
        {
            try
            {
                var file = await telegramBot.GetFileAsync(fileId);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await telegramBot.DownloadFileAsync(file.FilePath, stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error downloading: " + ex.Message);
            }
        }
    }
}
