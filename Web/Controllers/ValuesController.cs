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
    public ActionResult<Evalutation> Post([FromBody] Input value) {
      try {
        var eval = new Evalutation(value.text);
        return eval;
      } catch (Exception e) {
        return new Evalutation {
          Results = new[] { new Result(e.ToString()) },
          Error = true
        };
      }
    }

    public class Input
    {
      public string text { get; set; }
    }
  }
}
