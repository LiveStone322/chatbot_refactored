using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZulipAPI.BaseClasses;
using ZulipAPI.Messages;
using ZulipAPI.Streams;
using ZulipAPI.Users;
using ZulipAPI;
using RestSharp;
using RestSharp.Authenticators;
using WebApp.Models;
using Telegram.Bot;
using Viber.Bot;
using System.IO;
using Newtonsoft.Json;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZulipController : ControllerBase
    {
        static UserEndPoint userEndpoint;

        [HttpPost]
        public async Task<StatusCodeResult> Post()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
                body = await reader.ReadToEndAsync();
            dynamic data = Newtonsoft.Json.Linq.JObject.Parse(body)["data"];

            return Ok();
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public static async Task<StatusCodeResult> Send(string text, int userID, bool start = false)
        {
            userEndpoint = Zulip.zclient.GetUserEndPoint();
            var name = "user_" + userID;
            var email = name + "@healthapp.my.to";
            var _users = await userEndpoint.GetUsers();
            var _user = _users.FirstOrDefault(x => x.Email == email); ;
            if (start)
            {
                if ( _user == null)
                {                                                          //пароль нужен сильный
                    await userEndpoint.CreateUser(new ZulipAPI.User(email, "password51651651651651651", name, name));
                    _users = await userEndpoint.GetUsers();
                    _user = _users.FirstOrDefault(x => x.Email == email);
                }
            }

            try
            {
                var client = await new ZulipServer(AppInfo.ZulipServerURL).LoginAsync(_user.Email, "password51651651651651651");
                var messageEndpoint = client.GetMessageEndPoint();
                await messageEndpoint.SendStreamMessage("Medical things", "test-topic", text);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            return new StatusCodeResult(StatusCodes.Status200OK);
        }
    }

}