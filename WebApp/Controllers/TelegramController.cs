using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using System.Text.RegularExpressions;
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
            string nextMessage = "";
            List<Tuple<string, string>> entities = null;
            var text = message.Text;
            var intentList = Shared.NL.GetActionsFromText(text);
            if (intentList.Length == 0) return;

            var user = Shared.DBF.GetOrCreateUser(Sources.TELEGRAM,  message.Chat.Id, tlgrmUser.Username, tlgrmUser.FirstName);
            //возможно легче на dynamic cделать
            var parsedContext = user.GetParsedContext();

            var intent = intentList[0];

            entities = GetEntities(intent);

            var keywords = new List<nl_fhir.Keyword>();

            foreach (var i in intentList)
            {
                foreach (var k in i.Keywords)
                    keywords.Add(k);
            }

            switch (intent.Result)
            {
                case nl_fhir.ActionsEnum.Actions.ReadMyBiomarkers:
                    break;
                case nl_fhir.ActionsEnum.Actions.LoadFile:
                    break;
                case nl_fhir.ActionsEnum.Actions.ADD_BIOMARK:
                    break;
                case nl_fhir.ActionsEnum.Actions.GetPlot:
                    break;
                case nl_fhir.ActionsEnum.Actions.SendToApp:
                    break;
                case nl_fhir.ActionsEnum.Actions.ConnectToMobileApp:
                    break;
                case nl_fhir.ActionsEnum.Actions.CallHuman:
                    break;
                case nl_fhir.ActionsEnum.Actions.SecretMessage:
                    break;
                case nl_fhir.ActionsEnum.Actions.Answer:
                    break;
            }


            await Shared.telegramBot.SendTextMessageAsync(message.Chat.Id, "+");
        }

        private static List<Tuple<string, string>> GetEntities(nl_fhir.NLResult intent)
        {
            var entities = new List<Tuple<string, string>>();

            foreach (var k in intent.Keywords)
            {
                string regexp = Shared.DBF.GetEntityData(k.Value).Item1;
                var match = Regex.Match(intent.Text, regexp);
                string result = "";
                if (match.Groups.Count > 1)
                {
                    for (int i = 1; i < match.Groups.Count; i++)
                        result += match.Groups[i] + ";";
                    result = result.Substring(0, result.Length - 1);  //можно было бы сделать через While
                }
                else result = match.Value;
                entities.Add(new Tuple<string, string>(k.Value, result));
            }

            return entities;
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