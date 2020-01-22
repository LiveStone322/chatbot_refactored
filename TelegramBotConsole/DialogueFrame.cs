using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
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
        public string Entity { get; set; }  //пока используется только для записи ответа
        public object Tag { get; set; }   //здесь лежит id следующего вопроса или еще что-нибудь
        
        public DialogueFrame()
        {
            Activity = EnumActivity.Unknown;
            Entity = "";
        }

        public DialogueFrame(EnumActivity activity, string entity = "", object tag = null)
        {
            Activity = activity;
            Entity = String.Copy(entity);
            Tag = tag;
        }

        public static DialogueFrame GetDialogueFrame(Telegram.Bot.Args.MessageEventArgs e, HealthBotContext ctx, users dbUser)
        {

            EnumActivity ea;
            string ent = "";
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
            else if (txt == "/start")
            {
                ea = EnumActivity.ConversationStart;
            }
            else if (dbUser.id_last_question != null)
            {
                ea = EnumActivity.Answer;
                ent = String.Copy(txt);
                tag = FindNextQuestion(ctx, dbUser, dbUser.id_last_question.Value);
            }
            else ea = EnumActivity.Unknown;

            return new DialogueFrame(ea, ent, tag);
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
        public static string GetNextMessage(DialogueFrame df, HealthBotContext ctx, ref ReplyKeyboardMarkup keyboard)
        {
            string message = "";
            if (df.Activity == EnumActivity.Answer || df.Activity == EnumActivity.ReadMyBiomarkers)
            {
                if (df.Tag != null)
                {
                    message = ctx.Questions.Find(df.Tag).name;
                }
            }
            else if (df.Activity == EnumActivity.LoadFile)
                message = "Изображение сохранено";
            else if (df.Activity == EnumActivity.ConversationStart)
                message = "Хотите начать разговор?";

            //формирование кнопок
            if (df.Activity == EnumActivity.ConversationStart)
            {
                keyboard = new ReplyKeyboardMarkup
                {
                    Keyboard = new[] {
                            new[] //1 row
                            {
                                new KeyboardButton("Да"),
                                new KeyboardButton("Нет"),
                            },
                        },
                    ResizeKeyboard = true
                };
            }

            return message;
        }

        public static async void SendNextMessage(DialogueFrame df, HealthBotContext ctx, 
                                                            users dbUser, Chat chat, ITelegramBotClient client)
        {
            string message = "";
            ReplyKeyboardMarkup keyboard = null;

            if (df.Activity == EnumActivity.Unknown) return;

            message = GetNextMessage(df, ctx, ref keyboard);

            
            if (message != "")
            {
                dbUser.id_last_question = (int?)df.Tag;
                if (keyboard != null)
                {
                    await client.SendTextMessageAsync(
                      chatId: chat,
                      text: message,
                      replyMarkup: keyboard
                    );
                }
                else
                {
                    await client.SendTextMessageAsync(
                      chatId: chat,
                      text: message
                    );
                }
                
            }
            else dbUser.id_last_question = null;
        }
    }
}
