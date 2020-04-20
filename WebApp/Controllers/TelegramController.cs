using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using WebApp.Models;
using Newtonsoft.Json;

//dotnet publish -c Release -r linux-x64 --self-contained true
namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelegramController : ControllerBase
    {
        public async Task<StatusCodeResult> Post()
        {
            DialogueFrame df;
            string body;
            using (var reader = new StreamReader(Request.Body))
                body = await reader.ReadToEndAsync();

            var update =  JsonConvert.DeserializeObject<Update>(body);

            if (update.Type != Telegram.Bot.Types.Enums.UpdateType.Message)
                return Ok();

            Console.WriteLine(update);

            using (var ctx = new HealthBotContext())
            {
                var tlgrmUser = update.Message.From;
                var dbUser = ctx.users.Where(t => t.loginTelegram == tlgrmUser.Username).FirstOrDefault();

                if (dbUser == null) //если пользователя нет
                {
                    dbUser = new users()
                    {

                        loginTelegram = tlgrmUser.Username,
                        fio = tlgrmUser.FirstName + " " + tlgrmUser.LastName,
                        telegram_chat_id = update.Message.Chat.Id
                    };
                    ctx.users.Add(dbUser);
                }

                //обработка сообщения (Dialogue state tracker)
                df = DialogueFrame.GetDialogueFrame(update, ctx, dbUser);

                //внутренняя работа в рамках платформы
                switch (df.Activity)
                {
                    case DialogueFrame.EnumActivity.Answer:
                        await ctx.questions_answers.AddAsync(new questions_answers
                        {
                            id_user = dbUser.id,
                            id_question = dbUser.id_last_question.Value,
                            value = df.Entity
                        });
                        break;
                    case DialogueFrame.EnumActivity.LoadFile:
                        var path = Path.GetFullPath(@"..\..\");
                        var name = update.Message.Photo[update.Message.Photo.Length - 1].FileId;
                        DownloadFile(name, path + name);
                        ctx.files.Add(new files
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

                //обработка следующего сообщения (Dialogue state manager)
                DialogueFrame.SendNextMessage(df, ctx, dbUser, update.Message.Chat, Bots.telegramBot);
                await ctx.SaveChangesAsync();
            }

            return Ok();
        }

        private static async void DownloadFile(string fileId, string path)
        {
            try
            {
                var file = await Bots.telegramBot.GetFileAsync(fileId);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await Bots.telegramBot.DownloadFileAsync(file.FilePath, stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error downloading: " + ex.Message);
            }
        }
    }
}