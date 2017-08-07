using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IdentityServer4Demo1.Controllers
{

    [Route("identity")]
    [Authorize]
    public class IdentityController : ControllerBase
    {
        // GET: /<controller>/
        [HttpGet]
        public IActionResult Get()
        {
            //return View();
            return new JsonResult(from c in User.Claims select new { c.Type, c.Value });

        }
    }
}
