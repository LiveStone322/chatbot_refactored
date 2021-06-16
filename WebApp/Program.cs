using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ICQ.Bot;
using ICQ.Bot.Args;
using WebApp.Models;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RunNotificationsSender();

            InitializeSharedModels();

            RunBots();

            CreateHostBuilder(args).Build().Run();
        }

        private static void InitializeSharedModels()
        {
            Shared.NL = new nl_fhir.NLModel();
            Shared.DBF = new DBFacade();
        }

        private static void RunBots()
        {
            Shared.icqBot = new ICQBotClient(AppInfo.IcqToken);
            Shared.telegramBot = new Telegram.Bot.TelegramBotClient(AppInfo.TelegramToken);

            Shared.icqBot.OnMessage += BotOnMessageReceived;
            Shared.telegramBot.OnMessage += Controllers.TelegramController.Bot_OnMessage;

            Shared.telegramBot.StartReceiving();
            Shared.icqBot.StartReceiving();
        }

        private static void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {/*
            DialogueFrame df;
            var message = messageEventArgs.Message;

            using (var ctx = new HealthBotContext())
            {
                var icqUser = message.From;
                var dbUser = ctx.users.Where(t => t.loginIcq == icqUser.UserId).FirstOrDefault();

                if (dbUser == null) //если пользователя нет
                {
                    dbUser = new users()
                    {
                        // новые пользователи почему-то становятся в статус chatting
                        loginIcq = icqUser.UserId,
                        fio = icqUser.FirstName + " " + icqUser.LastName,
                        icq_chat_id = icqUser.UserId
                    };
                    ctx.users.Add(dbUser);
                }

                //обработка сообщения (Dialogue state tracker)
                df = DialogueFrame.GetDialogueFrame(message, ctx, dbUser);

                //доп. работа
                if (df.Activity == DialogueFrame.EnumActivity.DoNothing) return;
                switch (df.Activity)
                {
                    case DialogueFrame.EnumActivity.Answer:
                        ctx.questions_answers.Add(new questions_answers
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
                        break;
                    case DialogueFrame.EnumActivity.ReadMyBiomarkers:
                        dbUser.id_last_question = null;
                        dbUser.is_last_question_system = false;
                        break;
                    case DialogueFrame.EnumActivity.ConversationStart: break;
                    case DialogueFrame.EnumActivity.Unknown: break;
                }

                //обработка следующего сообщения (Dialogue state manager)
                DialogueFrame.SendNextMessage(df, ctx, dbUser, Shared.icqBot).Wait();
                ctx.SaveChanges();
            }

            return;*/
        }

        private static void RunNotificationsSender()
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(1);

            var timer = new System.Threading.Timer((e) =>
            {
                SendNotifications();
            }, null, startTimeSpan, periodTimeSpan);
        }

        private static void SendNotifications()
        {
            // todo
        }

        private static bool CheckTime(DateTime? datetime)
        {
            if (datetime.HasValue)
            {
                return datetime.Value.Minute == DateTime.Now.Minute &&
                    datetime.Value.Hour == DateTime.Now.Hour &&
                    datetime.Value.Day == DateTime.Now.Day &&
                    datetime.Value.Month == DateTime.Now.Month &&
                    datetime.Value.Year == DateTime.Now.Year;
                    
            }
            else return false;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
