using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    public enum DBContextTypeEnum
    {
        LastBotMessages,
        LastUserMessages,
        Mode
    }

    public class DBContextSerializable
    {
        public List<DBContextBase> Context { get; set; }

        public DBContextSerializable(List<DBContextBase> ctx)
        {
            Context = ctx;
        }
    }

    public class DBParsedContext
    {
        private DBContextTypeEnum[] notNullContext_;
        public DBContextTypeEnum[] notNullContext 
        {
            get
            {
                return notNullContext_;
            }
        }

        public DBCLastBotMessages[] lastBotMessages { get; set; }
        public DBCLastUserMessages[] lastUserMessages { get; set; }
        public DBCMode mode { get; set; }

        public DBParsedContext()
        {
            notNullContext_ = new DBContextTypeEnum[] { };
            lastBotMessages = null;
            lastUserMessages = null;
            mode = null;
        }

        public void UpdateNotNullContext()
        {
            var list = new List<DBContextTypeEnum>();
            if (lastBotMessages != null && lastBotMessages.Length > 0) list.Add(DBContextTypeEnum.LastBotMessages);
            if (lastUserMessages != null && lastUserMessages.Length > 0) list.Add(DBContextTypeEnum.LastUserMessages);
            if (mode != null) list.Add(DBContextTypeEnum.Mode);

            notNullContext_ = list.ToArray();
        }
    }

    public class DBContextBase
    {
        public DBContextTypeEnum Type { get; set; }
    }

    public class DBCLastBotMessages : DBContextBase
    {
        public string Test { get; set; } = "test!";
        public DBCLastBotMessages()
        {
            Type = DBContextTypeEnum.LastBotMessages;
        }
    }

    public class DBCLastUserMessages : DBContextBase
    {
        public DBCLastUserMessages()
        {
            Type = DBContextTypeEnum.LastUserMessages;
        }
    }

    public class DBCMode : DBContextBase
    {
        public DBCModeEnum Mode { get; set; }
        public DBCMode()
        {
            Type = DBContextTypeEnum.Mode;
            Mode = DBCModeEnum.UNKNOWN;
        }
    }

    public enum DBCModeEnum
    {
        UNKNOWN,
        ANSWER,
    }
}
