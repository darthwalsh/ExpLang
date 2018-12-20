using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // POST values
        [HttpPost]
        public ActionResult<string> Post([FromBody] Input value)
        {
            return value.text + " / " + DateTime.Now.ToString();
        }

        public class Input {
            public string text { get; set; }
        }
    }
}
