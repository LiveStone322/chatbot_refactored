using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApp.Models;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MisTestController : ControllerBase
    {
        [HttpGet]
        public string Get([FromQuery]string type)
        {
            try
            {

                if (type == "appointment")
                {
                    new MisController().Post(JsonConvert.SerializeObject(
                                                new MisUpdate<UpdateAppointment>()
                                                {
                                                    Update = UpdateType.Appointment,
                                                    UpdateMessage = new UpdateAppointment()
                                                    {
                                                        TelegramName = "LiveStoneArmy",
                                                        ViberName = "LiveStone",
                                                        Phone = "+79027916146",
                                                        DateTime = GetNextMinuteFromNow(),
                                                        Doctor = "Андреев А.А."
                                                    }
                                                }));
                    return "Отправлена запись";
                }
                else
                {
                    new MisController().Post(JsonConvert.SerializeObject(
                                                new MisUpdate<UpdateConclusion>()
                                                {
                                                    Update = UpdateType.Conclusion,
                                                    UpdateMessage = new UpdateConclusion()
                                                    {
                                                        TelegramName = "LiveStoneArmy",
                                                        ViberName = "LiveStone",
                                                        Phone = "+79027916146",
                                                        DateTime = GetNextMinuteFromNow(),
                                                        Cures = new[] { "Лекарственин", "Лечебнин" },
                                                        Measures = new[] { "мг", "мл" },
                                                        Dozes = new[] { 300, 200 },
                                                        Times = new[] { 1, 2 },
                                                        Periods = new[] { "день", "день" } //другие варианты пока не реализованы
                                                }
                                                }));
                    return "Отправлено заключение";
                }
            }
            catch(Exception ex)
            {
                return "Ошибка: \n" + ex.Message + '\n' + ex.InnerException;
            }
        }
        
        private static DateTime GetNextMinuteFromNow()
        {
            var now = DateTime.Now;
            return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute + 1, now.Second);
        }
    }
}
