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
            Answer,
            ConversationStart
        }

        public EnumActivity Activity { get; set; }  //действие пользователя, которое он от нас хочет
        //public string Entity { get; set; }  //не используется, но возможно
        public object Tag { get; set; }   //здесь лежит id следующего вопроса или еще что-нибудь
        
        public DialogueFrame()
        {
            Activity = EnumActivity.Unknown;
            //Entity = "";
        }

        public DialogueFrame(EnumActivity activity, object tag = null)
        {
            Activity = activity;
            //Entity = String.Copy(entity);
            Tag = tag;
        }

        public static DialogueFrame GetDialogueFrame(Telegram.Bot.Args.MessageEventArgs e, HealthBotContext ctx, users dbUser)
        {

            EnumActivity ea;
            //string ent = "";
            object tag = null;
            string txt;
            if (e.Message.Text == null) txt = e.Message.Caption.ToLower();
            else txt = e.Message.Text.ToLower();

            if (txt == "запиши мои показания")
            {
                ea = EnumActivity.ReadMyBiomarkers;
                tag = FindNextQuestion(ctx, dbUser);
            }
            else if (txt == "загрузи файл")
            {
                ea = EnumActivity.LoadFile;
                //доп контекст не нужен?
            }
            else if (dbUser.id_last_question != null)
            {
                ea = EnumActivity.Answer;
                //ent = String.Copy(txt);
                tag = FindNextQuestion(ctx, dbUser, dbUser.id_last_question.Value);
            }
            else ea = EnumActivity.Unknown;

            return new DialogueFrame(ea, tag);
        }

        //следующий вопрос
        private static int? FindNextQuestion(HealthBotContext ctx, users dbUser, int startIndex = -1)
        {
            var q =  ctx.Questions
                            .OrderBy(t => t.id)
                            .Where(t => t.id > startIndex)
                            .FirstOrDefault();
            if (q != null)
                return q.id;
            else return null;
        }


        //Tuple: string - next message; int - next message id
        public static string GetNextMessage(DialogueFrame df, HealthBotContext ctx, bool succeed = true)
        {
            if (df.Activity == EnumActivity.Answer || df.Activity == EnumActivity.ReadMyBiomarkers)
            {
                if (df.Tag != null)
                {
                    return ctx.Questions.Find(df.Tag).name;
                }
            }
            else if (df.Activity == EnumActivity.LoadFile)
                if (succeed) return "Изображение сохранено";
                else return "Во время сохранения произошла ошибка";
                
            return "";
        }

        public static async void SendNextMessage(DialogueFrame df, HealthBotContext ctx, 
                                                            users dbUser, Chat chat, ITelegramBotClient client, bool succeed)
        {
            string message = "";
            if (df.Activity == DialogueFrame.EnumActivity.Unknown) return;
            else if (df.Activity == DialogueFrame.EnumActivity.ReadMyBiomarkers)
            {
                message = GetNextMessage(df, ctx);
            }
            else if (df.Activity == DialogueFrame.EnumActivity.LoadFile)
            {
                if (succeed) message = GetNextMessage(df, ctx);
            }
            else if (df.Activity == DialogueFrame.EnumActivity.Answer)
            {
                if (succeed) message = GetNextMessage(df, ctx); 
            }

            if (message != "")
            {
                dbUser.id_last_question = (int?)df.Tag; //?
                await client.SendTextMessageAsync(
                  chatId: chat,
                  text: message
                );
            }
            else dbUser.id_last_question = null;
        }
    }
}
