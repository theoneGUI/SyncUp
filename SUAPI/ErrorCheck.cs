using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncUpAPI.Controllers {
    [ApiController]
    [Route("/error/errorCheck")]
    public class ErrorCheck : ControllerBase {
        [HttpGet]
        public string Get(string sessID, string status, string data)
        {
            if (status.Equals("GET"))
            {
                if (Universe.errors.ContainsKey(sessID))
                {
                    return Universe.errors[sessID];
                }
                else
                {
                    return "key_not_available";
                }
            }
            else if (status.Equals("SET"))
            {
                try
                {
                    if (Universe.openRequests.ContainsKey(sessID))
                    {
                        Universe.errors[sessID] = data;
                        return "set_success";
                    }
                    else
                        return "matchmaking_key_does_not_exist";
                }
                catch (Exception)
                {
                    return "set_error";
                }
            }
            else
            {
                return "not_valid_operation";
            }
        }


    }
}
