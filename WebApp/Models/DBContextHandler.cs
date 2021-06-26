using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApp.Models
{
    public static class DBContextHandler
    {
        private static JsonSerializerSettings jset = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

        public static List<DBContextBase> StringToContext(string json)
        {
            if (json == "") return null;

            var context = new List<DBContextBase>();
            //dynamic foo = JObject.Parse(request);
            //if (foo.Update == UpdateType.Appointment)
            //    var a = foo.message as UpdateAppointment;
            //var update = JsonConvert.DeserializeObject<MisUpdateMinimal>(request);

            try
            {
                var test = JsonConvert.DeserializeObject<List<DBContextBase>>(json, jset);
                dynamic jobj = JArray.Parse(json);
                for (var j = jobj.First; j != null; j = j.Next)
                {
                    context.Add((DBContextBase)((JObject)j).ToObject(typeof(DBCLastBotMessages)));
                }
                return context;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static string ContextToString(List<DBContextBase> obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
