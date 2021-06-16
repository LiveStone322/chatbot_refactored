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
using Telegram.Bot.Args;

//dotnet publish -c Release -r linux-x64 --self-contained true
namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelegramController : ControllerBase
    {
        public async Task<StatusCodeResult> Post()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
                body = await reader.ReadToEndAsync();

            var update =  JsonConvert.DeserializeObject<Update>(body);

            if (update.Type != Telegram.Bot.Types.Enums.UpdateType.Message)
                return Ok();

            ProcessMessage(update.Message.From, update.Message);

            return Ok();
        }

        public static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            ProcessMessage(e.Message.From, e.Message);
        }

        private static async void ProcessMessage(User tlgrmUser, Message message)
        {
            
            var text = message.Text;
            var answer = Shared.NL.GetNormalizedText(text);
            var intent = Shared.NL.GetActionFromText(text);

            Shared.DBF.AddUser(Sources.TELEGRAM, message.From.Username, message.Chat.Id, message.From.FirstName);

            await Shared.telegramBot.SendTextMessageAsync(message.Chat.Id, answer);

        }

        private static async void DownloadFile(string fileId, string path)
        {
            try
            {
                var file = await Shared.telegramBot.GetFileAsync(fileId);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await Shared.telegramBot.DownloadFileAsync(file.FilePath, stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error downloading: " + ex.Message);
            }
        }
    }
}