using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TelegramBotConsole.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UpdateType
    {
        Appointment = 1,
        Conclusion = 2
    }

    public class UpdateBase
    {
        [JsonProperty("telegram_name")]
        public string TelegramName { get; set; }
        [JsonProperty("viber_name")]
        public string ViberName { get; set; }
        [JsonProperty("phone")]
        public string Phone { get; set; }
        [JsonProperty("date_time")]
        public DateTime DateTime { get; set; }
    }

    public class UpdateAppointment : UpdateBase
    {
        [JsonProperty("doctor")]
        public string Doctor { get; set; }
    }
    public class UpdateConclusion : UpdateBase
    {
        [JsonProperty("cures")]
        public string[] Cures { get; set; }
        [JsonProperty("measures")]
        public string[] Measures { get; set; }
        [JsonProperty("dozes")]
        public int[] Dozes { get; set; }
        [JsonProperty("times")]
        public int[] Times { get; set; }
        [JsonProperty("periods")]
        public string[] Periods { get; set; }
        [JsonProperty("additional")]
        public string[] Additional { get; set; }
        [JsonProperty("recommendations")]
        public string[] Recommendations { get; set; }
    }

    public class MisUpdate<T>: MisUpdateMinimal  where T : UpdateBase
    {
        [JsonProperty("update_message")]
        public T UpdateMessage { get; set; }
    }

    public class MisUpdateMinimal
    {
        [JsonProperty("update_type")]
        public UpdateType Update { get; set; }
    }
}
