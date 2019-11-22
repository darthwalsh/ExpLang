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
    public ActionResult<Evaluation> Post([FromBody] Input value) {
      try {
        return new Evaluation(value.text);
      } catch (Exception e) {
        return new Evaluation {
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
