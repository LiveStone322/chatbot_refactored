using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Viber.Bot;
using WebApp.Models;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RunNotificationsSender();
            
            CreateHostBuilder(args).Build().Run();
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
            using (var ctx = new HealthBotContext())
            {
                var notificatedUsers = ctx.users.Where(t => ctx.notifications.Where(n => CheckTime(n.on_time)).Select(q => q.id).Contains(t.id)).Select(e => e.id);

            };
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
