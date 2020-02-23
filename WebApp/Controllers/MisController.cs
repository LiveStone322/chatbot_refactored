using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApp.Models;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MisController : ControllerBase
    {
        [HttpPost]
        public StatusCodeResult Post([FromBody]string request)
        {
            //dynamic foo = JObject.Parse(request);
            //if (foo.Update == UpdateType.Appointment)
            //    var a = foo.message as UpdateAppointment;
            //var update = JsonConvert.DeserializeObject<MisUpdateMinimal>(request);

            var update = JsonConvert.DeserializeObject<MisUpdateMinimal>(request);
            if (update.Update == UpdateType.Appointment)
            {
                var _update = JsonConvert.DeserializeObject<MisUpdate<UpdateAppointment>>(request);
                using (var ctx = new HealthBotContext())
                {
                    try
                    {
                        ctx.Notifications.Add(new notifications()
                        {
                            id_user = FindUser(_update.UpdateMessage, ctx),
                            message = CreateNotificationText(_update.UpdateMessage),
                            on_time = _update.UpdateMessage.DateTime
                        });
                        ctx.SaveChanges();
                    }
                    catch(ArgumentNullException)
                    {
                        return new StatusCodeResult(404);
                    }
                }
            }
            else
            {
                using (var ctx = new HealthBotContext())
                {
                    var _update = JsonConvert.DeserializeObject<MisUpdate<UpdateConclusion>>(request);

                    try
                    {
                        ctx.Notifications.Add(new notifications()
                        {
                            id_user = FindUser(_update.UpdateMessage, ctx),
                            message = "Принимайте лекарства",
                            on_time = GetTime(_update.UpdateMessage.DateTime)
                        });
                        ctx.SaveChanges();
                    }
                    catch (ArgumentNullException)
                    {
                        return new StatusCodeResult(404);
                    }
                }
            }
            return Ok();
        }

        private DateTime GetTime(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute + 1, dt.Second);
        }


        private string CreateNotificationText(UpdateAppointment message)
        {
            return "Вы записаны к доктору " + message.Doctor + " на " + message.DateTime.ToLongDateString() + ". Не опаздывайте";
        }

        private int FindUser(UpdateBase message, HealthBotContext ctx)
        {
            int? id = null;
            users user;
            if (message.ViberName == null) message.ViberName = "";
            if (message.TelegramName == null) message.TelegramName = "";
            if (message.ViberName.Length != 0 || message.TelegramName.Length != 0)
            {
                user = ctx.Users.FirstOrDefault(t => t.loginViber == message.ViberName ||
                                                            t.loginTelegram == message.TelegramName);
                if (user != null) id = user.id;
            }
            else
            {
                user = ctx.Users.FirstOrDefault(t => t.phone_number == message.Phone);
                if (user != null) id = user.id;
            }

            if (!id.HasValue) throw new ArgumentNullException();
            else return id.Value;
        }
    }
}