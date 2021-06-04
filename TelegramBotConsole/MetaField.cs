using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotConsole
{
    public enum MetaMorphType
    {
        Name,
        Surname,
        SecName,
        Noun,
        Adverb,
        Adjective
    }

    public enum MetaType
    {
        Number,
        Date,
        Biomark
    }

    class MetaField
    {
        public string Name { get; set; }
        public string Question { get; set; }
        public string Format { get; set; }
        public MetaMorphType Morph { get; set; }
        public MetaType Type { get; set; }
    }
}
