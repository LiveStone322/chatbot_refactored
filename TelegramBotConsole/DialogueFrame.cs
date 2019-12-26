using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Requests;
using System.Linq;

namespace TelegramBotConsole
{
    class DialogueFrame
    {
        public enum EnumActivity 
        {
            Unknown,
            ReadMyBiomarkers,
            LoadFile,
            Answer
        }

        public EnumActivity Activity { get; set; }
        public string Entity { get; set; } 
        
        public int? last_quest { get; set; }
        
        public DialogueFrame()
        {
            Activity = EnumActivity.Unknown;
            Entity = "";
        }

        public DialogueFrame(EnumActivity activity, string entity, int? last_q = null)
        {
            Activity = activity;
            Entity = String.Copy(entity);
            last_quest = last_q;
        }

        public static DialogueFrame GetDialogueFrame(Telegram.Bot.Args.MessageEventArgs e, HealthBotContext ctx, users dbUser)
        {

            EnumActivity ea;
            string ent = "";
            int? tag = null;
            string txt;
            if (e.Message.Text == null) txt = e.Message.Caption.ToLower();
            else txt = e.Message.Text.ToLower();

            if (txt == "запиши мои показания")
            {
                ea = EnumActivity.ReadMyBiomarkers;
            }
            else if (txt == "загрузи файл")
            {
                ea = EnumActivity.LoadFile;
                tag = -1;
                
            }
            else if (dbUser.id_last_question != null)
            {
                ea = EnumActivity.Answer;
                ent = String.Copy(txt);
                tag = dbUser.id_last_question;
            }
            else ea = EnumActivity.Unknown;

            return new DialogueFrame(ea, ent, tag);
        }

        //Tuple: string - next message; int - next message id
        public static Tuple<string, int> GetNextMessage(DialogueFrame df, HealthBotContext ctx)
        {
            if (df.Activity == EnumActivity.Answer || df.Activity == EnumActivity.ReadMyBiomarkers)
            {
                int id;
                if (df.last_quest == null) id = -1;
                else id = df.last_quest.Value;
                var next_quest = ctx.Questions.OrderBy(t => t.id).Where(t => t.id > id).FirstOrDefault(); //следующий

                if (next_quest != null)
                {
                    return new Tuple<string, int>(ctx.Questions.Find(next_quest.id).name, next_quest.id);
                }
            }
            else if (df.Activity == EnumActivity.Unknown) return new Tuple<string, int>("", -1);
            return new Tuple<string, int>("", -1);
        }
    }
}
