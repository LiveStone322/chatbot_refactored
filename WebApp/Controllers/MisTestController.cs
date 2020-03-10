using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApp.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MisTestController : ControllerBase
    {
        //telegram-Сообщение
        //
        [HttpGet]
        public async Task<string> GetAsync([FromQuery]string type)
        {
            var idx = type.LastIndexOf('-');
            string additional;
            Update update = null;

            if (idx != -1) additional = type.Substring(idx + 1);
            else additional = null;

            if (additional != null)
                update = new Update()
                {
                    Id = 111,
                    Message = new Message()
                    {
                        MessageId = 1,
                        From = new User()
                        {
                            Id = 1111,
                            FirstName = "Tester",
                            LastName = "Testerov",
                            Username = "@TestUser",
                            IsBot = false
                        },
                        Text = additional,
                        Chat = new Chat()
                        {
                            Id = 241186491,
                            Username = "@TestUSer",
                            Type = Telegram.Bot.Types.Enums.ChatType.Private,
                            FirstName = "Tester"
                        }
                    }
                };

            type = type.Substring(0, idx);
            try
            {
                switch (type)
                {
                    case "appointment":
                        new MisController().Post(JsonConvert.SerializeObject(
                                                new MisUpdate<UpdateAppointment>()
                                                {
                                                    Update = UpdateType.Appointment,
                                                    UpdateMessage = new UpdateAppointment()
                                                    {
                                                        TelegramName = "LiveStoneArmy",
                                                        ViberName = "LiveStone",
                                                        Phone = "+79027916146",
                                                        DateTime = DateTime.Now,
                                                        Doctor = "Андреев А.А."
                                                    }
                                                }));
                        return "Отправлена запись";
                    case "conclusion":
                        new MisController().Post(JsonConvert.SerializeObject(
                                                new MisUpdate<UpdateConclusion>()
                                                {
                                                    Update = UpdateType.Conclusion,
                                                    UpdateMessage = new UpdateConclusion()
                                                    {
                                                        TelegramName = "LiveStoneArmy",
                                                        ViberName = "LiveStone",
                                                        Phone = "+79027916146",
                                                        DateTime = DateTime.Now,
                                                        Cures = new[] { "Лекарственин", "Лечебнин" },
                                                        Measures = new[] { "мг", "мл" },
                                                        Dozes = new[] { 300, 200 },
                                                        Times = new[] { 1, 2 },
                                                        Periods = new[] { "день", "день" } //другие варианты пока не реализованы
                                                    }
                                                }));
                        return "Отправлено заключение";
                    case "telegram":
                        if (update != null)
                        {
                            await new TelegramController().Post(update);
                            return "Отправлено: " + additional;
                        }
                        else return "update было равно null. Правильно ли вы отправили запрос: " + type + " : "+ additional + "?";
                    default: return "type is appointment, conclusion or telegram";
                }
            }
            catch(Exception ex)
            {
                return "Ошибка: \n" + ex.Message + '\n' + ex.InnerException;
            }
        }
    }
}
