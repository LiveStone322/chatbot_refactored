using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApp.Models;
using Telegram.Bot;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MisController : ControllerBase
    {
            //dynamic foo = JObject.Parse(request);
            //if (foo.Update == UpdateType.Appointment)
            //    var a = foo.message as UpdateAppointment;
            //var update = JsonConvert.DeserializeObject<MisUpdateMinimal>(request);

        private DateTime GetTime(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute + 1, dt.Second);
        }


        private string CreateNotificationText(UpdateAppointment message)
        {
            return "Вы записаны к доктору " + message.Doctor + " на " + message.DateTime.ToLongDateString() + ". Не опаздывайте";
        }
    }
}