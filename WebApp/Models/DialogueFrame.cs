using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Viber.Bot;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using ICQ.Bot;
using System.Text.RegularExpressions;
using ZulipAPI;
using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace WebApp
{
    class BiomarkForApp
    {
        public string token;
        public DateTime begin;
        public DateTime end;
        public string parameterName;

        public BiomarkForApp(string token, DateTime begin, DateTime end, string parameterName)
        {
            this.token = token;
            this.begin = begin;
            this.end = end;
            this.parameterName = parameterName;
        }
    }

    class NextMessage
    {
        //сделать
    }

    class DialogueFrame
    {
        public enum EnumActivity 
        {
            Unknown,
            ReadMyBiomarkers,
            LoadFile,
            Answer,
            ConversationStart,
            SecretMessage,
            ConversationStartAnswer,
            GetPlot,
            ConnectToMobileApp,
            DoNothing,
            SystemAnswer,
            SendToApp,
            CallHuman,
            Chatting,
            AddBiomarks,
            PrintBiomarks
        }

        public enum SystemMessages
        {
            Hello = 1,  
            ImageSaved = 2,
            WantToStart = 3,   
            SendBiomark_plot = 4,    
            NoBiomarks = 5,
            SendToken = 6,  
            SecretMessage = 7,  
            YouveEnteredToken = 8, 
            ThanksForToken = 9,
            SendBiomark_record = 11, 
            SendBiomarkValue_record = 12,    
            PleaseEnterNumber = 13,
            YouDontHaveBiomarksToTrack = 14,
            ErrorInputBiomark = 15,
            PleaseListBiomarks = 16,
            Success = 17,
            WhatPeriod = 18
        }

        public EnumActivity Activity { get; set; }  //действие пользователя, которое он от нас хочет
        public string Entity { get; set; }  //пока используется только для записи ответа
        public object Tag { get; set; }   //здесь лежит id следующего вопроса или еще что-нибудь

        public DialogueFrame()
        {
            var client =
            Activity = EnumActivity.Unknown;
            Entity = "";
        }

        public DialogueFrame(EnumActivity activity, string entity = "", object tag = null)
        {
            Activity = activity;
            Entity = string.Copy(entity);
            Tag = tag;
        }

        //viber
        public static DialogueFrame GetDialogueFrame(CallbackData e, HealthBotContext ctx, users dbUser)
        {
            string txt;
            if (e.Message.Type == MessageType.Text)
                txt = ((TextMessage)e.Message).Text;
            else if (e.Message.Type == MessageType.Picture)
                return new DialogueFrame(EnumActivity.LoadFile);//txt = ((PictureMessage)e.Message).Text;
            else return new DialogueFrame(EnumActivity.Unknown);

            return AnylizeMessage(txt, ctx, dbUser);
        }

        //telegram
        public static DialogueFrame GetDialogueFrame(Update e, HealthBotContext ctx, users dbUser)
        {
            string txt;
            if (e.Message.Text == null) txt = e.Message.Caption.ToLower();  //лучше смотреть тип сообщения
            else txt = e.Message.Text.ToLower();
            return AnylizeMessage(txt, ctx, dbUser);
        }

        //icq
        public static DialogueFrame GetDialogueFrame(ICQ.Bot.Types.Message e, HealthBotContext ctx, users dbUser)
        {
            string txt;
            txt = e.Text.ToLower();
            return AnylizeMessage(txt, ctx, dbUser);
        }

        public static string FindActivityEntityInTable(string[] words, HealthBotContext ctx)
        {
            knowledge_base row;
            if (words.Length > 1)
                foreach (var first in words)
                    foreach (var second in words)
                    {
                        row = ctx.knowledge_base.Where(t => t.activity == first && t.entity == second).FirstOrDefault();
                        if (row != null) return row.output;
                    }
            else foreach (var first in words)   //такое себе, но а вдруг 0 элементов
                {
                    row = ctx.knowledge_base.Where(t => t.entity == first).FirstOrDefault();
                    if (row != null) return row.output;
                }

            return null;
        }


        private static DialogueFrame AnylizeMessage(string message, HealthBotContext ctx, users dbUser)
        {
            EnumActivity ea = EnumActivity.DoNothing;
            string ent = "";
            object tag = null;

            if(dbUser.chatting!=null && dbUser.chatting != "")
            {
                ea = EnumActivity.Chatting;
                ent = message;
            }
            else
            {
                var txt = FindActivityEntityInTable(message.Replace(",", "").Replace(".", "").Replace(":", "").Split(' '), ctx);
                if (txt == null) txt = message;

                if (txt == "запиши мои показания")
                {
                    ea = EnumActivity.ReadMyBiomarkers;
                    tag = FindNextQuestion(ctx, dbUser);
                    if (tag == null) ent = ctx.system_messages.Find((int)SystemMessages.YouDontHaveBiomarksToTrack).message;
                }
                else if (txt == "загрузи файл")
                {
                    ea = EnumActivity.LoadFile;
                    //доп контекст не нужен?
                }
                else if (txt == "добавь показатели")
                {
                    ea = EnumActivity.AddBiomarks;
                }
                else if (txt == "/start")
                {
                    ea = EnumActivity.ConversationStart;
                }
                else if (txt == "покажи мне график приложения")
                {
                    ea = EnumActivity.GetPlot;
                }
                else if (txt == "введи показатель в приложение")
                {
                    ea = EnumActivity.SendToApp;
                }
                else if (txt == "синхронизируй с приложением")
                {
                    ea = EnumActivity.ConnectToMobileApp;
                }
                else if (txt == "позови человека")
                {
                    ea = EnumActivity.CallHuman;
                }
                else if (txt == "выведи показания")
                {
                    ea = EnumActivity.PrintBiomarks;
                }
                else if (txt == "секретное сообщение")
                {
                    ea = EnumActivity.SecretMessage;
                }
                else if (dbUser.id_last_question != null && dbUser.is_last_question_system.HasValue)
                {
                    //ответ на запись показаний
                    if (!dbUser.is_last_question_system.Value)
                    {
                        var question = ctx.biomarks.Where(t => t.id == dbUser.id_last_question).FirstOrDefault();
                        if (question != null)
                            ent = ParseAnswer(txt, question.format, question.splitter);
                        if (ent != "")
                        {
                            ea = EnumActivity.Answer;
                            tag = FindNextQuestion(ctx, dbUser, dbUser.id_last_question.Value);
                        }
                        else
                        {
                            ea = EnumActivity.Unknown;
                            ent = ctx.system_messages.Find((int)SystemMessages.ErrorInputBiomark).message;
                            tag = true;
                        }
                    }
                    else
                    {
                        //системное сообщение
                        bool makeNull = false;
                        switch (dbUser.id_last_question)
                        {
                            case (int)SystemMessages.WantToStart:
                                if (txt == "да") ea = EnumActivity.DoNothing;
                                else
                                {
                                    ea = EnumActivity.Unknown;
                                    ent = "Неправильный ответ";
                                }
                                makeNull = true;
                                break;
                            case (int)SystemMessages.SendBiomark_plot:
                                ea = EnumActivity.SystemAnswer;
                                ent = txt;
                                break;
                            case (int)SystemMessages.SendBiomark_record:
                                ea = EnumActivity.SystemAnswer;
                                ent = txt;
                                break;
                            case (int)SystemMessages.SendBiomarkValue_record:
                                ea = EnumActivity.SystemAnswer;
                                ent = txt;
                                break;
                            case (int)SystemMessages.SendToken:
                                ea = EnumActivity.SystemAnswer;
                                ent = txt;
                                break;
                            case (int)SystemMessages.YouveEnteredToken:
                                if (txt == "да")
                                {
                                    ea = EnumActivity.SystemAnswer;
                                    ent = txt;
                                }
                                else if (txt == "нет") ea = EnumActivity.DoNothing;
                                else
                                {
                                    ea = EnumActivity.Unknown;
                                    ent = "Ошибка при вводе. Повторите снова";
                                }
                                break;
                            case (int)SystemMessages.PleaseListBiomarks:
                                ea = EnumActivity.SystemAnswer;
                                ent = txt;
                                break;
                            case (int)SystemMessages.WhatPeriod:
                                ea = EnumActivity.SystemAnswer;
                                ent = txt;
                                break;
                        }
                        if (ea == EnumActivity.DoNothing || ea == EnumActivity.Unknown)
                        {
                            dbUser.is_last_question_system = null;
                            dbUser.id_last_question = null;
                        }
                    }
                }
                else ea = EnumActivity.Unknown;
            }

            return new DialogueFrame(ea, ent, tag);
        }

        private static string ParseAnswer(string txt, string format, string splitter)
        {
            var match = Regex.Match(txt, format);
            string result = "";
            if (match.Groups.Count > 1)
            {
                for (int i = 1; i<match.Groups.Count; i++)
                    result += match.Groups[i] + splitter;
                result = result.Substring(0, result.Length - splitter.Length);  //можно было бы сделать через While
            }
            else result = match.Value;
            return result;
        }

        private static int? FindNextQuestion(HealthBotContext ctx, users dbUser, int startIndex = -1)
        {
            var qs = GetUsersBiomarks(dbUser, ctx).Select(t => t.id);
            var q = ctx.biomarks
                            .OrderBy(t => t.id)
                            .Where(t => t.id > startIndex && qs.Contains(t.id))
                            .FirstOrDefault();
            
            if (q != null)
            {
                dbUser.is_last_question_system = false;
                return q.id;
            }
            else
            {
                dbUser.is_last_question_system = null;
                return null;
            }
        }


        public static async Task<string> GetNextMessage(DialogueFrame df, users dbUser, HealthBotContext ctx, string[] buttons)
        {
            string message = "";
            if (df.Activity == EnumActivity.CallHuman)
            {
                await SendToZulip(dbUser, ctx, "С Вами общается " + dbUser.fio, true);
                message = "Ожидание оператора. Для отмены напишите \"Отмена\". (прим. автора: операторов нет, поэтому скорее всего ждать придется очень долго)";
                dbUser.id_last_question = null;
                dbUser.is_last_question_system = null;
            }
            else if (df.Activity == EnumActivity.Chatting)
            {
                if (df.Entity.ToLower() == "отмена")
                {
                    dbUser.chatting = "";
                    message = "Диалог закончен. Спасибо";
                }
                await SendToZulip(dbUser, ctx, df.Entity, false);
            }
            else if (df.Activity == EnumActivity.AddBiomarks)
            {
                message = ctx.system_messages.Find((int)SystemMessages.PleaseListBiomarks).message;
                dbUser.id_last_question = (int)SystemMessages.PleaseListBiomarks;
                dbUser.is_last_question_system = true;
            }
            else if (df.Activity == EnumActivity.PrintBiomarks)
            {
                message = ctx.system_messages.Find((int)SystemMessages.WhatPeriod).message;
                dbUser.id_last_question = (int)SystemMessages.WhatPeriod;
                dbUser.context = ((int)EnumActivity.PrintBiomarks).ToString();
                dbUser.is_last_question_system = true;
            }
            else if (df.Activity == EnumActivity.Answer || df.Activity == EnumActivity.ReadMyBiomarkers)
            {
                if (df.Tag != null)
                {
                    message = ctx.questions.Where(t => t.id_biomark == (int?)df.Tag).FirstOrDefault().question;
                    dbUser.id_last_question = (int?)df.Tag;
                }
                else return message;
            }
            else if (df.Activity == EnumActivity.SystemAnswer)
            {
                switch (dbUser.id_last_question)
                {
                    case (int)SystemMessages.SendBiomark_plot:
                        message = App_GetPlot(dbUser.token, DateTime.MinValue, DateTime.Now, dbUser.context);
                        if (message.Length == 0 || message == "К сожалению, приложение \"Здоровье\" не отвечает. попробуйте позже.")
                        {
                            dbUser.id_last_question = null;
                            dbUser.is_last_question_system = null;
                        }
                        break;
                    case (int)SystemMessages.SendToken:
                        message = ctx.system_messages.Find((int)SystemMessages.ThanksForToken).message;
                        dbUser.token = df.Entity;
                        dbUser.id_last_question = null;
                        dbUser.is_last_question_system = null;
                        break;
                    case (int)SystemMessages.YouveEnteredToken:
                        message = ctx.system_messages.Find((int)SystemMessages.SendToken).message;
                        dbUser.id_last_question = (int)SystemMessages.SendToken;
                        break;
                    case (int)SystemMessages.SendBiomark_record:
                        dbUser.context = df.Entity;
                        message = ctx.system_messages.Find((int)SystemMessages.SendBiomarkValue_record).message;
                        dbUser.id_last_question = (int)SystemMessages.SendBiomarkValue_record;
                        dbUser.is_last_question_system = true;
                        break;
                    case (int)SystemMessages.SendBiomarkValue_record:
                        double value;
                        if (double.TryParse(df.Entity.Replace(',', '.'), out value))
                        {
                            message = App_AddRecord(dbUser.token, dbUser.context, DateTime.Now, value.ToString());
                            dbUser.id_last_question = null;
                            dbUser.is_last_question_system = null;
                        }
                        else
                        {
                            message = ctx.system_messages.Find((int)SystemMessages.PleaseEnterNumber).message;
                            dbUser.id_last_question = (int)SystemMessages.SendBiomarkValue_record;
                            dbUser.is_last_question_system = true;
                        }
                        break;
                    case (int)SystemMessages.PleaseListBiomarks:
                        try
                        {
                            var biomarks = df.Entity.Split(", ");
                            biomarks dbB = null;
                            foreach (var b in biomarks)
                            {
                                dbB = ctx.biomarks.Where(t => t.name == b).FirstOrDefault();
                                if (dbB != null && !ctx.users_biomarks.Where(t => t.id_user == dbUser.id).Select(t => t.id_biomark).Contains(dbB.id)) 
                                    ctx.users_biomarks.Add(new users_biomarks() { id_user = dbUser.id, id_biomark = dbB.id });
                            }
                            message = ctx.system_messages.Find((int)SystemMessages.Success).message;
                            dbUser.id_last_question = null;
                            dbUser.is_last_question_system = null;
                        }
                        catch (Exception e)
                        {
                            message = e.Message;
                            dbUser.id_last_question = null;
                            dbUser.is_last_question_system = null;
                        }
                        break;
                    case (int)SystemMessages.WhatPeriod:
                        {
                            if (dbUser.context == ((int)EnumActivity.PrintBiomarks).ToString())
                                switch(df.Entity)
                                {
                                    case "сутки":
                                        message = PrintBiomarks(dbUser, ctx, 1);
                                        dbUser.id_last_question = null;
                                        dbUser.is_last_question_system = null;
                                        break;
                                    case "неделя":
                                        message = PrintBiomarks(dbUser, ctx, 7);
                                        dbUser.id_last_question = null;
                                        dbUser.is_last_question_system = null;
                                        break;
                                    case "месяц":
                                        message = PrintBiomarks(dbUser, ctx, 30);
                                        dbUser.id_last_question = null;
                                        dbUser.is_last_question_system = null;
                                        break;
                                    default: 
                                        message = "Ошибка при вводе перида";
                                        dbUser.id_last_question = null;
                                        dbUser.is_last_question_system = null;
                                        break;
                                }
                            if (message == "") message = "Нет показателей для вывода. Для записи показателей напшите \"Запиши мои показатели\"";
                            dbUser.context = "";
                            break;
                        }
                }
            }
            else if (df.Activity == EnumActivity.LoadFile)
                message = ctx.system_messages.Find((int)SystemMessages.ImageSaved).message;
            else if (df.Activity == EnumActivity.ConversationStart)
            {
                buttons = new[] { "Да", "Нет" };
                message = ctx.system_messages.Find((int)SystemMessages.WantToStart).message;
                dbUser.id_last_question = (int)SystemMessages.WantToStart;
                dbUser.is_last_question_system = true;
            }
            else if (df.Activity == EnumActivity.ConversationStartAnswer)
                message = ctx.system_messages.Find((int)SystemMessages.Hello).message;
            else if (df.Activity == EnumActivity.SendToApp)
            {
                message = ctx.system_messages.Find((int)SystemMessages.SendBiomark_record).message;
                dbUser.id_last_question = (int)SystemMessages.SendBiomark_record;
                dbUser.is_last_question_system = true;
            }
            else if (df.Activity == EnumActivity.GetPlot)
            {
                message = ctx.system_messages.Find((int)SystemMessages.SendBiomark_plot).message;
                dbUser.id_last_question = (int)SystemMessages.SendBiomark_plot;
                dbUser.is_last_question_system = true;
            }
            else if (df.Activity == EnumActivity.ConnectToMobileApp)
                if (dbUser.token == null || dbUser.token.Length == 0)
                {
                    message = ctx.system_messages.Find((int)SystemMessages.SendToken).message;
                    dbUser.id_last_question = (int)SystemMessages.SendToken;
                    dbUser.is_last_question_system = true;
                }
                else
                {
                    message = ctx.system_messages.Find((int)SystemMessages.YouveEnteredToken).message;
                    dbUser.id_last_question = (int)SystemMessages.YouveEnteredToken;
                    dbUser.is_last_question_system = true;
                }
            else if (df.Activity == EnumActivity.SecretMessage)
            {
                message = ctx.system_messages.Find((int)SystemMessages.SecretMessage).message;
                //message = App_AddRecord(dbUser.token, "шаги", DateTime.Now, "10.5");
                //message = App_GetPlot(dbUser.token, DateTime.MinValue, DateTime.Now, "шаги");
            }
            else if (df.Activity == EnumActivity.Unknown)
                message = df.Entity;

            return message;
        }

        private static string PrintBiomarks(users dbUser, HealthBotContext ctx, int t)
        {
            //var table = from questions_answers in ctx.questions_answers
            //            join biomarks in ctx.biomarks on questions_answers.id_question equals biomarks.id
            //            where (questions_answers.id_user == dbUser.id && ((DateTime.Now - questions_answers.date_time).TotalHours / 24) <= t)
            //            select new
            //            {
            //                name = biomarks.name,
            //                value = questions_answers.value
            //            };
            var time = DateTime.Now.Subtract(new TimeSpan(t, 0, 0, 0));
            var bs = ctx.questions_answers
                    .Where(qa => qa.id_user == dbUser.id && qa.date_time >= time)
                    .Join(ctx.biomarks, z => z.id_question, b => b.id, (z, b) => new
                    {
                        name = b.name,
                        value = z.value
                    })
                    .OrderBy(z => z.name)
                    .ToList();
            string message = "";
            foreach (var b in bs)
                message += b.name + ": " + b.value + "\n";
            if (message != "") return message.Substring(0, message.Length - 1);
            return message;
        }

        private static async Task SendToZulip(users dbUSer, HealthBotContext ctx, string txt, bool createUser)
        {
            await Controllers.ZulipController.Send(txt, dbUSer.id, createUser);
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

        private static biomarks[] GetUsersBiomarks(users dbUser, HealthBotContext ctx)
        {
            return ctx.biomarks.Where(t => ctx.users_biomarks.Where(q => q.id_user == dbUser.id).Select(z => z.id_biomark).Contains(t.id)).ToArray();
        }

        public static async Task SendNextMessage(DialogueFrame df, HealthBotContext ctx,
                                                            users dbUser, Chat chat, ITelegramBotClient client)
        {
            string message = "";
            ReplyKeyboardMarkup keyboard;
            string[] buttons = null;

            if (df.Activity == EnumActivity.Unknown)
                if (df.Entity != "") message = df.Entity;
                else return;
            else 
            {
                message = await GetNextMessage(df, dbUser, ctx, buttons);
                if (df.Activity == EnumActivity.CallHuman)
                {
                    dbUser.id_last_question = null;
                    dbUser.is_last_question_system = null;
                    dbUser.chatting = "telegram";
                }
                if (dbUser.is_last_question_system.HasValue)
                {
                    //если нужно прислать картинку
                    if (dbUser.is_last_question_system.Value == true && dbUser.id_last_question == (int)SystemMessages.SendBiomark_plot)
                    {
                        using (Stream stream = System.IO.File.OpenRead(dbUser.token + ".png"))
                        {
                            await client.SendPhotoAsync(
                                chatId: chat,
                                photo: stream,
                                caption: "Ваш график"
                                );
                        }
                        dbUser.id_last_question = null;
                        dbUser.is_last_question_system = null;
                    }
                }
                if (message != "")
                {
                    if (buttons != null)
                    {
                        keyboard = new ReplyKeyboardMarkup
                        {
                                Keyboard = new[] {
                                    buttons.Select(t => new Telegram.Bot.Types.ReplyMarkups.KeyboardButton(t))
                                },
                                ResizeKeyboard = true
                        };
                        await client.SendTextMessageAsync(
                          chatId: chat,
                          text: message,
                          replyMarkup: keyboard
                        );
                    }
                    else
                    {
                        await client.SendTextMessageAsync(
                          chatId: chat,
                          text: message,
                          replyMarkup: new ReplyKeyboardRemove()
                        );
                    }
                }
                else dbUser.id_last_question = null;
            }
        }

        public static async void SendNextMessage(DialogueFrame df, HealthBotContext ctx, users dbUser, 
                                                        CallbackData callbackData, IViberBotClient client)
        {
            string message = "";
            Keyboard keyboard;
            string[] buttons = null;

            if (df.Activity == EnumActivity.Unknown)
                if (df.Entity != "") message = df.Entity;
                else return;
            else
            {
                message = await GetNextMessage(df, dbUser, ctx, buttons);
                if (df.Activity == EnumActivity.CallHuman)
                {
                    dbUser.id_last_question = null;
                    dbUser.is_last_question_system = null;
                    dbUser.chatting = "viber";
                }
                if (dbUser.is_last_question_system.HasValue)
                {
                    //если нужно прислать картинку
                    if (dbUser.is_last_question_system.Value == true && dbUser.id_last_question == (int)SystemMessages.SendBiomark_plot)
                    {
                        using (Stream stream = System.IO.File.OpenRead(dbUser.token + ".png"))
                        {
                            await client.SendPictureMessageAsync(
                                    new PictureMessage() {
                                        Text = "Ваш график",
                                        Receiver = callbackData.Sender.Id,
                                        MinApiVersion = callbackData.Message.MinApiVersion,
                                        TrackingData = callbackData.Message.TrackingData,
                                        Media = "https://upload.wikimedia.org/wikipedia/commons/5/57/Viber-logo.png" //viber is lame
                                    }
                                );
                        }
                        dbUser.id_last_question = null;
                        dbUser.is_last_question_system = null;
                    }
                }
                if (message != "")
                {
                    dbUser.id_last_question = (int?)df.Tag;
                    if (buttons != null)
                    {
                        keyboard = new Keyboard()
                        {
                            BackgroundColor = "#32C832",
                            Buttons = buttons.Select(t => new Viber.Bot.KeyboardButton() { Text = t }).ToList()
                        };
                        await client.SendKeyboardMessageAsync(new KeyboardMessage
                        {
                            Text = message,
                            Keyboard = keyboard,
                            Receiver = callbackData.Sender.Id,
                            MinApiVersion = callbackData.Message.MinApiVersion,
                            TrackingData = callbackData.Message.TrackingData
                        });
                    }
                    else
                    {
                        await client.SendTextMessageAsync(new TextMessage()
                        {
                            Text = message,
                            Receiver = callbackData.Sender.Id,
                            MinApiVersion = callbackData.Message.MinApiVersion,
                            TrackingData = callbackData.Message.TrackingData
                        });
                    }

                }
                else dbUser.id_last_question = null;
            }
        }

        public static async Task SendNextMessage(DialogueFrame df, HealthBotContext ctx, users dbUser, IICQBotClient client)
        {
            string message = "";
            ICQ.Bot.Types.ReplyMarkups.InlineKeyboardMarkup keyboard;
            string[] buttons = null;

            if (df.Activity == EnumActivity.Unknown)
                if (df.Tag != null) 
                    await client.SendTextMessageAsync(
                          chatId: dbUser.icq_chat_id,
                          text: df.Entity,
                          replyMarkup: null
                        );
                else return;
            else
            {
                message = await GetNextMessage(df, dbUser, ctx, buttons);
                if (df.Activity == EnumActivity.CallHuman)
                {
                    dbUser.id_last_question = null;
                    dbUser.is_last_question_system = null;
                    dbUser.chatting = "icq";
                }
                if (dbUser.is_last_question_system.HasValue)
                {
                    //если нужно прислать картинку
                    //if (dbUser.is_last_question_system.Value && dbUser.id_last_question == (int)SystemMessages.SendBiomark_plot)
                    //{
                    //    using (Stream stream = System.IO.File.OpenRead(dbUser.token + ".png"))
                    //    {
                    //        await client.SendFileAsync(
                    //            chatId: dbUser.icq_chat_id,
                    //            document: new ICQ.Bot.Types.InputFiles.InputOnlineFile(stream),
                    //            caption: "Ваш график"
                    //            );
                    //    }
                    //    dbUser.id_last_question = null;
                    //    dbUser.is_last_question_system = null;
                    //}
                }
                if (message != "")
                {
                    if (buttons != null)
                    {
                        keyboard = new ICQ.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(
                                buttons.Select(t => new ICQ.Bot.Types.ReplyMarkups.InlineKeyboardButton() { Text = t })
                            );

                        await client.SendTextMessageAsync(
                          chatId: dbUser.icq_chat_id,
                          text: message,
                          replyMarkup: keyboard
                        );
                    }
                    else
                    {
                        await client.SendTextMessageAsync(
                          chatId: dbUser.icq_chat_id,
                          text: message,
                          replyMarkup: null
                        );
                    }
                }
                else dbUser.id_last_question = null;
            }
        }
    }
}
