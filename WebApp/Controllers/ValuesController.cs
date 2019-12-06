using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Viber.Bot;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // POST 
        [HttpPost]
        public string Post()
        {
            using (var reader = new StreamReader(Request.Body))
                return reader.ReadToEnd();
        }

    }
}
