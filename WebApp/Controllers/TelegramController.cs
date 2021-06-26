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
            Tuple<string, string>[] entities = null;
            Tuple<string, string>[] existingEntities = null;
            Tuple<string, string>[] nonExistingEntities = null;
            var text = message.Text;
            var intentList = Shared.NL.GetActionsFromText(text);
            if (intentList.Length == 0) return;

            var user = Shared.DBF.GetOrCreateUser(Sources.TELEGRAM,  message.Chat.Id, tlgrmUser.Username, tlgrmUser.FirstName);
            //возможно легче на dynamic cделать
            var parsedContext = user.GetParsedContext();

            var intent = intentList[0];


            var neededEntities = Shared.DBF.GetNeededEntities(intent.Result.ToString());

            if (neededEntities.Length > 0)
            {
                entities = GetEntities(intent).Where(t => t.Item2 != null).ToArray();
                existingEntities = entities.Where(t => neededEntities.Any(n => n.Item1 == t.Item1)).Concat(parsedContext.entities.Value).ToArray();
                nonExistingEntities = neededEntities.Where(t => !entities.Any(n => n.Item1 == t.Item1)).ToArray();
            }

            if (nonExistingEntities.Length > 0)
            {
                nextMessage = NeedMoreEntitiesMessage(existingEntities, nonExistingEntities);
            }
            else
            {
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
            }

            user.SetContextElement(DBContextTypeEnum.Entities, existingEntities);

            await Shared.telegramBot.SendTextMessageAsync(message.Chat.Id, "+");
        }

        private static string NeedMoreEntitiesMessage(Tuple<string, string>[] existingEntities, Tuple<string, string>[] nonExistingEntities)
        {
            var got = "";
            if (existingEntities.Length > 0)
            {
                got += "Понял. Значит\n";
                foreach (var e in existingEntities)
                {
                    got += $"{e.Item1}: {e.Item2}\n";
                }
                got += "\n";
            }
            var more = existingEntities.Length > 0 ? "Но нужно уточнить еще немного:" : "Необходимо еще уточнить:\n"
                + string.Join("\n", nonExistingEntities.Select(t => t.Item2).ToArray()).ToLowerInvariant();
            return got + more;
        }

        private static List<Tuple<string, string>> GetEntities(nl_fhir.NLResult intent)
        {
            var entities = new List<Tuple<string, string>>();
            foreach (var k in intent.Keywords)
            {
                string regexp = Shared.DBF.GetEntityData(k.Value)?.Item1 ?? "(.+)";
                var right = GetFirstEntityFromArray(
                        intent.Text.Substring(k.Position + k.Value.Length, intent.Text.Length - k.Position - k.Value.Length).Trim().Split(' '),
                        regexp
                    );
                var left = GetFirstEntityFromArray(
                        intent.Text.Substring(0, k.Position).Trim().Split(' ').Reverse().ToArray(),
                        regexp
                    );

                entities.Add(new Tuple<string, string>(k.Value, right ?? left));
            }

            return entities;
        }

        private static string GetFirstEntityFromArray(string[] values, string regexp)
        {
            for (int i = 0; i < values.Length && i < 2; i++)
            {
                var match = Regex.Match(values[i], regexp);
                if (match.Success && match.Captures[0].Value.Length >= values[0].Length - 1) return values[0];
            }
            return null;
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