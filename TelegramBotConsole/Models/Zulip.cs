﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZulipAPI;
using ZulipAPI.Messages;

namespace TelegramBotConsole.Models
{
    public class Zulip
    {
        public static ZulipClient zclient { get; set; }
    }
}
