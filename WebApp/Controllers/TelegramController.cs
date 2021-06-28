using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using WebApp.Models;
using Newtonsoft.Json;
using Telegram.Bot.Args;
using System.Net;
using System.Text;

//dotnet publish -c Release -r linux-x64 --self-contained true
namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelegramController : ControllerBase
    {
        private static double thresholdScore = 0.8;
        private static double thresholdConfidence = 0.6;
        public async Task<StatusCodeResult> Post()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
                body = await reader.ReadToEndAsync();

            var update =  JsonConvert.DeserializeObject<Update>(body);

            if (update.Type != Telegram.Bot.Types.Enums.UpdateType.Message)
                return Ok();

            var result = await ProcessMessage(update.Message.From, update.Message);
            if (result.Item2 != "") await Shared.telegramBot.SendTextMessageAsync(update.Message.Chat.Id, result.Item2);

            return Ok();
        }

        public static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            var result = await ProcessMessage(e.Message.From, e.Message);
            if (result.Item2 != "") await Shared.telegramBot.SendTextMessageAsync(e.Message.Chat.Id, result.Item2);
        }
    }
}