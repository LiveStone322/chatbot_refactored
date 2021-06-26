using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.Models
{
    public class DBUser
    {
        public int Id { get; set; }
        public string Fio { get; set; }
        public string Phone { get; set; }
        public string Token { get; set; }
        public List<DBContextBase> Context { get; set; }
        public string ContextString
        {
            get
            {
                return DBContextHandler.ContextToString(Context);
            }
        }
        public string Username { get; set; }
        public int ChatId { get; set; }
        public Sources Source { get; set; }
        public int IdInSource { get; set; }

        public DBUser(int id, string fio, string phone, List<DBContextBase> context, string token, string username, int chatid, Sources source, int idInSource)
        {
            Id = id;
            Fio = fio;
            Phone = phone;
            Token = token;
            Context = context != null ? context : new List<DBContextBase>();
            Username = username;
            ChatId = chatid;
            Source = source;
            IdInSource = idInSource;
        }

        public void SetContext(List<DBContextBase> newContext)
        {
            Context = newContext;

            Shared.DBF.SetContext(Id, ContextString);
        }

        public void AddToContext(DBContextBase newContext)
        {
            Context.Add(newContext);

            Shared.DBF.SetContext(Id, ContextString);
        }
        
        public DBContextBase GetFirstFromContext(DBContextTypeEnum target)
        {
            return Context.FirstOrDefault(t => t.Type == target);
        }

        public DBContextBase[] GetAllFromContext(DBContextTypeEnum target)
        {
            return Context.Where(t => t.Type == target).ToArray();
        }

        public DBParsedContext GetParsedContext()
        {
            var parsed = new DBParsedContext();

            parsed.entities = (DBCEntities)GetFirstFromContext(DBContextTypeEnum.Entities);
            parsed.lastBotMessages = GetAllFromContext(DBContextTypeEnum.LastBotMessages).Select(t => (DBCLastBotMessages)t).ToArray();
            parsed.lastUserMessages = GetAllFromContext(DBContextTypeEnum.LastUserMessages).Select(t => (DBCLastUserMessages)t).ToArray();
            parsed.mode = (DBCMode)GetFirstFromContext(DBContextTypeEnum.Mode);

            parsed.UpdateNotNullContext();

            return parsed;
        }

        public void SetContextElement(DBContextTypeEnum type, object value)
        {
            foreach (var c in Context)
            {
                if (c.Type == type)
                {
                    ((dynamic)c).Value = value;
                    SetContext(Context);
                    return;
                }
            }
            AddToContext(GetNewBaseContext(type, value));
        }

        public DBContextBase GetNewBaseContext(DBContextTypeEnum type, object value = null)
        {
            DBContextBase db = null;
            switch(type)
            {
                case DBContextTypeEnum.Entities:
                    db = new DBCEntities();
                    if (value != null) ((dynamic)db).Value = value;
                    break;
                case DBContextTypeEnum.LastBotMessages:
                    db = new DBCLastBotMessages();
                    if (value != null) ((dynamic)db).Value = value;
                    break;
                case DBContextTypeEnum.LastUserMessages:
                    db = new DBCLastUserMessages();
                    if (value != null) ((dynamic)db).Value = value;
                    break;
                case DBContextTypeEnum.Mode:
                    db = new DBCMode();
                    if (value != null) ((dynamic)db).Value = value;
                    break;
            }
            return db;
        }
    }
}
