using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Viber.Bot;
using TelegramBotConsole.Models;

namespace TelegramBotConsole.Controllers
{
    public class ViberController
    {
        public async Task<StatusCodeResult> Post()
        {
            DialogueFrame df;

            if (callbackData.Event != EventType.Message) return Ok();

            using (var ctx = new HealthBotContext())
            {
                var viberUser = callbackData.Sender;
                var dbUser = ctx.users.Where(t => t.loginViber == viberUser.Id).FirstOrDefault(); //изменить БД
                if (AppInfo.isDebugging) Console.WriteLine("В БД ЗАШЛО");

                if (dbUser == null) //если пользователя нет
                {
                    if (AppInfo.isDebugging) Console.WriteLine("ДОБАВЛЯЮ ПОЛЬЗОВАТЕЛЯ В БД");
                    dbUser = new users()
                    {
                        loginViber = viberUser.Id,
                        fio = viberUser.Name
                    };
                    ctx.users.Add(dbUser);
                }
                //обработка сообщения (Dialogue state tracker)
                df = DialogueFrame.GetDialogueFrame(callbackData, ctx, dbUser);
                if (AppInfo.isDebugging) Console.WriteLine("СООБЩЕНИЕ ПОНЯТНО");

                if (df.Activity == DialogueFrame.EnumActivity.DoNothing) return Ok();
                switch (df.Activity)
                {
                    case DialogueFrame.EnumActivity.Answer:
                        await ctx.questions_answers.AddAsync(new questions_answers
                        {
                            id_user = dbUser.id,
                            id_question = dbUser.id_last_question.Value,
                            value = df.Entity,
                            date_time = DateTime.Now
                        });
                        break;
                    case DialogueFrame.EnumActivity.SystemAnswer:
                        break;
                    case DialogueFrame.EnumActivity.LoadFile:
                        if (AppInfo.isDebugging) Console.WriteLine("EnumActivity.LoadFile");
                        {
                            var path = Path.GetFullPath(@"..\");
                            if (AppInfo.isDebugging) Console.WriteLine("ПУТЬ: " + path);
                            var uri = ((PictureMessage)callbackData.Message).Media;
                            var name = uri.Substring(uri.LastIndexOf('/') + 1, uri.LastIndexOf('.') - uri.LastIndexOf('/') - 1);
                            if (AppInfo.isDebugging) Console.WriteLine("ИМЯ: " + name);

                            using (var client = new WebClient())
                            {
                                client.DownloadFile(uri, path + name);
                            }

                            ctx.files.Add(new files
                            {
                                content_hash = name,
                                directory = "test",
                                id_user = dbUser.id,
                                file_name = name,
                                file_format = "jpg",
                                id_source = 2
                            });
                            await ctx.SaveChangesAsync();

                            await Bots.viberBot.SendTextMessageAsync(new TextMessage()
                            {
                                Text = "Изображение сохранено",
                                Sender = new UserBase()
                                {
                                    Avatar = "https://dl-media.viber.com/1/share/2/long/vibes/icon/image/0x0/3eb4/af60406bde950b540663e6cad3f92ed79970d0ca6a6291491da8b00c3f643eb4.jpg",
                                    Name = "HealhBot"
                                },
                                Receiver = callbackData.Sender.Id,
                                MinApiVersion = callbackData.Message.MinApiVersion,
                                TrackingData = callbackData.Message.TrackingData
                            });
                            break;
                        }
                        break;
                    case DialogueFrame.EnumActivity.ReadMyBiomarkers:
                        dbUser.id_last_question = null;
                        dbUser.is_last_question_system = false;
                        break;
                    case DialogueFrame.EnumActivity.ConversationStart: break;
                    case DialogueFrame.EnumActivity.Unknown: break;
                }

                //обработка следующего сообщения (Dialogue state manager)
                DialogueFrame.SendNextMessage(df, ctx, dbUser, callbackData, Bots.viberBot);
                await ctx.SaveChangesAsync();
            }
            return Ok();
        }

    }
}
