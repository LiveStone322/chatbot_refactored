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
        static IViberBotClient viberBot;

        static void Main()
        {
            telegramBot = new TelegramBotClient(AppInfo.TelegramToken, new HttpToSocks5Proxy(AppInfo.Socks5Host, AppInfo.Socks5Port));
            viberBot = new ViberBotClient(AppInfo.ViberToken);


            //viber
            try
            {
                var viber = viberBot.GetAccountInfoAsync().Result;
                Console.WriteLine(
                  $"{viber.Name} работает."
                );
            }
            catch (Exception e) { Console.WriteLine("Ошибка при запуске Viber бота: " + e.Message); }
            //viberBot.SetWebhookAsync("http://healthapp.my.to/api/values", new System.Collections.Generic.List<EventType>() {
            //    EventType.Message
            //});

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
                        id = tlgrmUser.Id,
                        login = tlgrmUser.Username,
                        fio = tlgrmUser.FirstName + " " + tlgrmUser.LastName,
                    };
                    ctx.Users.Add(dbUser);
                    await ctx.SaveChangesAsync();
                }

                //обработка сообщения (Dialogue state tracker)
                df = DialogueFrame.GetDialogueFrame(e, ctx, dbUser);

                //доп. обработка
                if (df.Activity == DialogueFrame.EnumActivity.Unknown) return;
                if (df.Activity == DialogueFrame.EnumActivity.ReadMyBiomarkers)
                    dbUser.id_last_question = null;

                if (df.Activity == DialogueFrame.EnumActivity.LoadFile)
                {
                    var path = Path.GetFullPath(@"..\..\");

                    var name = e.Message.Photo[e.Message.Photo.Length - 1].FileId;
                    DownloadFile(name, path + name);
                    ctx.Files.Add(new files
                    {
                        content_hash = name,
                        directory = "test",
                        id_user = dbUser.id,
                        file_name = DateTime.Now.ToString(),
                        file_format = "jpg",
                        id_source = 1
                    });

                    await telegramBot.SendTextMessageAsync(
                          chatId: e.Message.Chat,
                          text: "Изображение сохранено"
                        );

                    await ctx.SaveChangesAsync();
                }

                if (df.Activity == DialogueFrame.EnumActivity.Answer)
                {
                    if (df.last_quest == null) throw new Exception("Последнего вопроса не было");
                    await ctx.Questions_answers.AddAsync(new questions_answers
                    {
                        id_user = dbUser.id,
                        id_question = df.last_quest.Value,
                        value = df.Entity
                    });

                    //обработка ответа (Dialogue manager)
                    var nextMessage = DialogueFrame.GetNextMessage(df, ctx);

                    if (nextMessage.Item2 != -1)
                    {
                        dbUser.id_last_question = nextMessage.Item2;
                        await telegramBot.SendTextMessageAsync(
                          chatId: e.Message.Chat,
                          text: ctx.Questions.Find(nextMessage.Item2).name
                        );
                    }
                    else dbUser.id_last_question = null;
                    await ctx.SaveChangesAsync();
                }
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
