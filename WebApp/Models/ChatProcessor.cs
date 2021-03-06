using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using WebApp.Models;
using System.Net;
using System.Text;

namespace WebApp.Models
{
    public class ChatProcessor
    {
        private static async Task<Tuple<nl_fhir.NLResult, string>> ProcessMessage(User tlgrmUser, Message message)
        {
            string nextMessage = "";
            Tuple<string, string>[] entities = null;
            Tuple<string, string>[] existingEntities = null;
            Tuple<string, string>[] nonExistingEntities = null;
            Tuple<string, string>[] neededEntities = null;
            Tuple<string, string> lookingFor = null;
            nl_fhir.NLResult[] intentList = null;
            nl_fhir.NLResult intent = null;

            var text = message.Text;
            var user = Shared.DBF.GetOrCreateUser(Sources.TELEGRAM, message.Chat.Id, tlgrmUser.Username, tlgrmUser.FirstName);
            var parsedContext = user.GetParsedContext();

            if (parsedContext.chating.Value)
            {
                SendToZulip(user, text, false);
                return new Tuple<nl_fhir.NLResult, string>(intent, "");
            }

            if (parsedContext.lookingFor != null)
            {
                string answer = TryGetAnswer(text, parsedContext.lookingFor.Value[0].Item2);
                if (answer != "")
                {
                    intent = new nl_fhir.NLResult(nl_fhir.ActionsEnum.Actions.Answer, text, 1, 1);
                    lookingFor = new Tuple<string, string>(parsedContext.lookingFor.Value[0].Item1, answer);
                }
            }

            if (intent != null)
            {
                var listEnt_ = new List<Tuple<string, string>>(parsedContext.entities.Value);
                listEnt_.Add(lookingFor);
                user.SetContextElement(DBContextTypeEnum.Entities, listEnt_);
                nextMessage = GetNextMessage(intent, user);
            }
            else
            {
                intentList = Shared.NL.GetActionsFromText(text);
                if (intentList.Length == 0) return null;

                //возможно легче на dynamic cделать

                intent = intentList[0];
                if (intent.Result == nl_fhir.ActionsEnum.Actions.ReadMyBiomarkers)
                    neededEntities = Shared.DBF.GetUserBiomarksSubscribtion(user);
                else neededEntities = Shared.DBF.GetNeededEntities(intent.Result.ToString());

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
                    nextMessage = GetNextMessage(intent, user);
                }

                user.SetContextElement(DBContextTypeEnum.Entities, existingEntities);
            }
            return new Tuple<nl_fhir.NLResult, string>(intent, nextMessage);

        }

        private static string GetNextMessage(nl_fhir.NLResult intent, DBUser user)
        {
            var context = user.GetParsedContext();
            switch (intent.Result)
            {
                case nl_fhir.ActionsEnum.Actions.ADD_BIOMARK:
                    var addedBiomarks = "";
                    if (context.entities.Value.Length == 0)
                    {
                        user.SetContextElement(DBContextTypeEnum.LastBotMessages, nl_fhir.ActionsEnum.Actions.ADD_BIOMARK);
                        return "Введите название показателя и его значение...";
                    }
                    foreach (var e in context.entities.Value)
                    {
                        Shared.DBF.AddEntity(user.Id, e.Item1, e.Item2);
                        addedBiomarks += $"{e.Item1}: {e.Item2}; ";
                    }
                    return "Добавлено!\n" + addedBiomarks;
                case nl_fhir.ActionsEnum.Actions.GetPlot:
                    return App_GetPlot(user.Token, DateTime.MinValue, DateTime.Now, context.entities.Value[0].Item1);
                case nl_fhir.ActionsEnum.Actions.SendToApp:
                    return App_AddRecord(user.Token, context.entities.Value[0].Item1, DateTime.Now, context.entities.Value[0].Item2);
                case nl_fhir.ActionsEnum.Actions.ConnectToMobileApp:
                    user.Token = context.entities.Value[0]?.Item2 ?? "";
                    if (user.Token == "") return "Ошибка. Не найден токен. Попробуйте ввести его снова";
                    else
                    {
                        Shared.DBF.SetUser(user);
                        return "Успешно!";
                    }
                case nl_fhir.ActionsEnum.Actions.CallHuman:
                    user.SetContextElement(DBContextTypeEnum.Chatting, true);
                    return "Ожидание оператора. Для отмены напишите 'Отмена'";
                case nl_fhir.ActionsEnum.Actions.SecretMessage: // для отладки
                    return "Введено секретное сообщение";
                case nl_fhir.ActionsEnum.Actions.FHIR_OBSERVATION:
                    if (context.entities.Value.Length == 0)
                    {
                        user.SetContextElement(DBContextTypeEnum.LastBotMessages, nl_fhir.ActionsEnum.Actions.FHIR_OBSERVATION);
                        return "Введите название показателя и его значение...";
                    }
                    var addedObs = "";
                    foreach (var e in context.entities.Value)
                    {
                        nl_fhir.Fhir.AddObservable(user.Fio, e.Item1, e.Item2);
                        addedObs += $"{e.Item1}: {e.Item2}; ";
                    }
                    return "Добавлено!\n" + addedObs;
                case nl_fhir.ActionsEnum.Actions.Answer:
                    switch (context.lookingFor.Value[0]?.Item1)
                    {
                        case "СОГЛАСИЕ":
                            break;
                        default:
                            Shared.DBF.AddEntity(user.Id, context.lookingFor.Value[0]?.Item1, context.lookingFor.Value[0]?.Item2);
                    }
                    break;
            }
            return "";
        }
        private static string PrintBiomarks(DBUser user, int t)
        {
            var time = DateTime.Now.Subtract(new TimeSpan(t, 0, 0, 0));
            var bs = Shared.DBF.GetUsersBiomarksValues(user);
            string message = "";
            foreach (var b in bs)
                message += b.name + ": " + b.value + "\n";
            if (message != "") return message.Substring(0, message.Length - 1);
            return message;
        }

        private static async Task SendToZulip(DBUser user, string txt, bool createUser)
        {
            await Controllers.ZulipController.Send(txt, user.Id, createUser);
        }

        private static string App_GetResponse(string url, string method, Dictionary<string, string> param, string filename = "error.png")
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url + "?" + string.Join("&", param.Select(pp => pp.Key + "=" + pp.Value)));
            HttpWebResponse resp;
            req.Method = method;
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch
            {
                return "К сожалению, приложение \"Здоровье\" не отвечает. попробуйте позже.";
            }
            if (resp.ContentType.StartsWith("image"))
            {
                using (Stream output = System.IO.File.OpenWrite(filename))
                using (Stream input = resp.GetResponseStream())
                {
                    input.CopyTo(output);
                }
                return "";
            }
            else
            {
                string result = "";
                using (Stream input = resp.GetResponseStream())
                {
                    int count = 0;
                    byte[] buf = new byte[8192];
                    do
                    {
                        count = input.Read(buf, 0, buf.Length);
                        if (count != 0)
                            result += Encoding.UTF8.GetString(buf, 0, count);
                    }
                    while (count > 0);
                }

                if (result == "") return "No output";
                return result;
            }
        }

        private static string App_AddRecord(string tokenValue, string parameterName, DateTime strDate, string strValue)
        {
            var postParams = new Dictionary<string, string>();
            postParams.Add("tokenValue", "asdasdasd");
            postParams.Add("parameterName", parameterName);
            postParams.Add("strDate", strDate.ToString());
            postParams.Add("strValue", strValue);

            return App_GetResponse(@"http://285f7a81.ngrok.io/main/AddRecordToUser", "POST", postParams);
        }

        private static string App_GetPlot(string tokenValue, DateTime strBegin, DateTime strEnd, string parameterName)
        {
            var postParams = new Dictionary<string, string>();
            postParams.Add("tokenValue", tokenValue);
            postParams.Add("strBegin", strBegin.ToString());
            postParams.Add("strEnd", strEnd.ToString());
            postParams.Add("parameterName", parameterName);

            return App_GetResponse(@"http://285f7a81.ngrok.io/main/getplot", "GET", postParams, tokenValue + ".png");

        }

        private static string TryGetAnswer(string text, string value)
        {
            var match = Regex.Match(text, value);
            if (match.Success && match.Captures[0].Value.Length >= text.Length - 1) return match.Captures[0].Value;
            return "";
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
