using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Viber.Bot;
using Telegram.Bot;
using ICQ.Bot;

namespace WebApp.Models
{
    public class Shared
    {
        public static readonly double threshold = 0.8;
        public static IViberBotClient viberBot { get; set; }
        public static ITelegramBotClient telegramBot { get; set; }
        public static IICQBotClient icqBot { get; set; }

        public static nl_fhir.NLModel NL { get; set; }

        public static DBFacade DBF { get; set; }
    }
}
