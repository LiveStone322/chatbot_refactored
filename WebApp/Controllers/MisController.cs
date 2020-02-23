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
                        ctx.notifications.Add(new notifications()
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
                        ctx.notifications.Add(new notifications()
                        {
                            id_user = FindUser(_update.UpdateMessage, ctx),
                            message = "Принимайте лекарства",
                            on_time = _update.UpdateMessage.DateTime
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

        private string CreateNotificationText(UpdateAppointment message)
        {
            return "Вы записаны к доктору " + message.Doctor + " на " + message.DateTime.ToLongDateString() + ". Не опаздывайте";
        }

        private string FindUser(UpdateBase message, HealthBotContext ctx)
        {
            string id = null;
            if (message.ViberName.Length != 0 || message.TelegramName.Length != 0)
                id = ctx.Users.FirstOrDefault(t => t.login == message.ViberName ||
                                                            t.login == message.TelegramName).id;
            else
                id = ctx.Users.FirstOrDefault(t => t.phone_number == message.Phone).id;

            if (id == null) throw new ArgumentNullException();
            return id;
        }
    }
}