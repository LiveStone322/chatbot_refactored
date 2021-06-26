using System;
using System.Collections.Generic;
using System.Text;

namespace WebApp.Models
{
    public static class DBDictionaries
    {
        public static readonly Dictionary<Sources, string> sourcesDic = new Dictionary<Sources, string>()
        {
            {Sources.UNKNOWN, ""},
            {Sources.TELEGRAM, "telegram"},
            {Sources.VIBER, "viber"},
            {Sources.ICQ, "icq"},
        };
    }
}
