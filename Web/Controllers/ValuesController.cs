using System;
using Engine;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // POST values
        [HttpPost]
        public ActionResult<Output> Post([FromBody] Input value) {
            try {
                var eval = new Evalutation(value.text);
                return new Output {
                    result = eval.Result,
                    error = eval.Error
                };
            } catch (Exception e) {
                return new Output {
                    result = e.ToString(),
                    error = true
                };
            }
        }

        public class Input
        {
            public string text { get; set; }
        }

        public class Output
        {
            public string result { get; set; }
            public bool error { get; set; }
        }
    }
}
