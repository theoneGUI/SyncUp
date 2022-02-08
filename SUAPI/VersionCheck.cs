using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncUpAPI.Controllers {
    [ApiController]
    [Route("/versions/check")]
    public class VersionCheck : ControllerBase {
        [HttpGet]
        public string Get(string assembly)
        {
            return Universe.VersionLookup(assembly);
        }

    }
}
