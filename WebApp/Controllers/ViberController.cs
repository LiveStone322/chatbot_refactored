using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Viber.Bot;
using WebApp.Models;

/// <summary>
/// TODO: где-то контекст переиспользуется????
/// </summary>
namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViberController : ControllerBase
    {
        //string reqTxt = "{\"event\":\"message\",\"timestamp\":1577087362822,\"chat_hostname\":\"SN-CHAT-01_\",\"message_token\":5389804735894462827,\"sender\":{\"id\":\"xp8uiNXBuhVQ3nTbdQlBYw==\",\"name\":\"Anton\",\"language\":\"ru\",\"country\":\"RU\",\"api_version\":7},\"message\":{\"text\":\"\u0437\u0430\u043F\u0438\u0448\u0438 \u043C\u043E\u0438 \u043F\u043E\u043A\u0430\u0437\u0430\u043D\u0438\u044F\",\"type\":\"text\"},\"silent\":false}";
        //string reqPhoto = "{\"event\": \"message\",\"timestamp\": 1577270319525,\"chat_hostname\": \"SN-CHAT-01_\",\"message_token\": 5390572111929636000,\"sender\": {\"id\": \"xp8uiNXBuhVQ3nTbdQlBYw==\",\"name\": \"Anton\",\"language\": \"ru\",\"country\": \"RU\",\"api_version\": 7},\"message\": {\"type\": \"picture\",\"media\": \"https://sun9-11.userapi.com/c855736/v855736568/13159f/_BpcXjL_sAQ.jpg\",\"thumbnail\": \"https://dl-media.viber.com/5/media/2/short/any/sig/image/400/ad7b/f5300cc4a02f5247fca5302671405dc8d6d9ffd07f7d1e519ae610b84f1dad7b.jpg?Expires=1577273920&Signature=Il7I74PvYCCkFrQoBHYx71~NEtVya88XsNkoKfRcKJHdVRxCCLH2isNBTJ8dfNg4BKJOzm16GoR2fZDG-T8Kdd2APwrxyRsqcuIvyLUPABsVdd14YtTXyBC2kjrmwPkxTnimKzTlR5Z963In0Xzub5WY4qEiggC80rIEoY7bQmrWDbp-ath3hc59e5-unLtI9NMiAx1YpCaq6AWuvQwlwvH23BsxQKZyBkBX3g1EjFHpL4q9Sk6~h1t1dz3ScFCV41dC9TvzNbWsPZhNSyOM7MEAblSNfhY8Fq~VpGDTkCwpT8WjLDWlAzX6sdtXohjygwpg2PJwMY3urJZsPlJOVw__&Key-Pair-Id=APKAJ62UNSBCMEIPV4HA\",\"file_name\": \"0Yupg0Qgnok.jpg\",\"size\": 356135},\"silent\": false}";

        [HttpGet]
        public string Get()
        {
            return $"HEALTH BOT";
        }

        // POST 
        [HttpPost]
        public async Task<StatusCodeResult> Post()
        {
            if (AppInfo.isDebugging) Console.WriteLine("-----ПРИШЛО СООБЩЕНИЕ----");
            string body;
            using (var reader = new StreamReader(Request.Body))
                body = await reader.ReadToEndAsync();
            //body = reqTxt;
            DialogueFrame df;
            if (AppInfo.isDebugging) Console.WriteLine("СООБЩЕНИЕ СЧИТАЛОСЬ");
            if (AppInfo.isDebugging) Console.WriteLine("СООБЩЕНИЕ: " + body);

            //запись body:
            //var fstream = new StreamWriter("../../../debug.txt");
            //fstream.WriteLine(body);
            //fstream.Close();

            //какая-то проверка
            //var isSignatureValid = ViberBot.viberBot.ValidateWebhookHash(Request.Headers["X-Viber-Content-Signature"], body);
            //if (!isSignatureValid)
            //{
            //    return new StatusCodeResult(StatusCodes.Status406NotAcceptable);
            //}
            var callbackData = JsonConvert.DeserializeObject<CallbackData>(body);
            if (AppInfo.isDebugging) Console.WriteLine("СООБЩЕНИЕ ДЕСЕРИАЛИЗОВАЛОСЬ");

            if (callbackData.Event != EventType.Message) return Ok();

            using (var ctx = new HealthBotContext())
            {
                var viberUser = callbackData.Sender;
                var dbUser = ctx.Users.Where(t => t.loginViber == viberUser.Id).FirstOrDefault(); //изменить БД
                if (AppInfo.isDebugging) Console.WriteLine("В БД ЗАШЛО");

                if (dbUser == null) //если пользователя нет
                {
                    if (AppInfo.isDebugging) Console.WriteLine("ДОБАВЛЯЮ ПОЛЬЗОВАТЕЛЯ В БД");
                    dbUser = new users()
                    {
                        loginViber = viberUser.Id,
                        fio = viberUser.Name,
                    };
                    ctx.Users.Add(dbUser);
                }
                //обработка сообщения (Dialogue state tracker)
                df = DialogueFrame.GetDialogueFrame(callbackData, ctx, dbUser);
                if (AppInfo.isDebugging) Console.WriteLine("СООБЩЕНИЕ ПОНЯТНО");

                //внутренняя работа
                switch (df.Activity)
                {
                    case DialogueFrame.EnumActivity.Unknown:
                        if (AppInfo.isDebugging) Console.WriteLine("EnumActivity.Unknown");
                        return Ok();
                    case DialogueFrame.EnumActivity.ReadMyBiomarkers:
                        if (AppInfo.isDebugging) Console.WriteLine("EnumActivity.ReadMyBiomarkers");
                        dbUser.id_last_question = null;
                        break;
                    case DialogueFrame.EnumActivity.ConversationStart:
                        if (AppInfo.isDebugging) Console.WriteLine("EnumActivity.ConversationStart"); 
                        break; //доделать
                    case DialogueFrame.EnumActivity.Answer:
                        if (AppInfo.isDebugging) Console.WriteLine("EnumActivity.Answer");
                        //if (AppInfo.isDebugging) Console.WriteLine($"ID: {dbUser.id} ID_QUESTION: {(int)df.Tag} VALUE: {df.Entity}");
                        await ctx.Questions_answers.AddAsync(new questions_answers
                        {
                            id_user = dbUser.id,
                            id_question = dbUser.id_last_question.Value,
                            value = df.Entity
                        });
                        break;
                    case DialogueFrame.EnumActivity.SecretMessage:
                        if (AppInfo.isDebugging) Console.WriteLine("EnumActivity.SecretMessage");
                        AppInfo.isDebugging = !AppInfo.isDebugging; 
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

                            ctx.Files.Add(new files
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
                }



                //обработка следующего сообщения (Dialogue state manager)
                DialogueFrame.SendNextMessage(df, ctx, dbUser, callbackData, Bots.viberBot);
                await ctx.SaveChangesAsync();
            }
            return Ok();
        }

    }
}
