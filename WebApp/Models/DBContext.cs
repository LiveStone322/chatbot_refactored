using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    public enum DBContextTypeEnum
    {
        Entities,
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

        public DBCEntities entities { get; set; }
        public DBCLastBotMessages[] lastBotMessages { get; set; }
        public DBCLastUserMessages[] lastUserMessages { get; set; }
        public DBCMode mode { get; set; }

        public DBParsedContext()
        {
            notNullContext_ = new DBContextTypeEnum[] { };
            entities = null;
            lastBotMessages = null;
            lastUserMessages = null;
            mode = null;
        }

        //after context update
        public void UpdateNotNullContext()
        {
            var list = new List<DBContextTypeEnum>();
            if (entities != null) list.Add(DBContextTypeEnum.Entities);
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

    public class DBCEntities : DBContextBase
    {
        public Tuple<string, string>[] Value { get; set; }
        public DBCEntities()
        {
            Type = DBContextTypeEnum.Entities;
            Value = new Tuple<string, string>[] { };
        }
    }

    public class DBCLastBotMessages : DBContextBase
    {
        public string Value { get; set; }
        public DBCLastBotMessages()
        {
            Type = DBContextTypeEnum.LastBotMessages;
            Value = "";
        }
    }

    public class DBCLastUserMessages : DBContextBase
    {
        public string Value { get; set; }
        public DBCLastUserMessages()
        {
            Type = DBContextTypeEnum.LastUserMessages;
            Value = "";
        }
    }

    public class DBCMode : DBContextBase
    {
        public DBCModeEnum Value { get; set; }
        public DBCMode()
        {
            Type = DBContextTypeEnum.Mode;
            Value = DBCModeEnum.UNKNOWN;
        }
    }

    public enum DBCModeEnum
    {
        UNKNOWN,
        ANSWER,
    }
}
