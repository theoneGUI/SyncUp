using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncUpAPI.Controllers {
    [ApiController]
    [Route("/auth/userauth")]
    public class UserAuth : ControllerBase {
        [HttpGet]
        public string Get(string hash, string PIP, string LIP, string status)
        {
            if (hash == null)
            {
                return "no user key given to authenticate";
            }
            else if (UserAuthMethods.hashExists(hash))
            {
                /*
                 * after user determined authentic, set the request to 'open' status in requests dictionary
                 * Universe.openRequests formatting:
                 * Key: random session ID
                 * Value[0]: username hash (provided by hash variable)
                 * Value[1]: public ip
                 * Value[2]: local ip
                 * Value[3]: "SEND" or "RECV" to indicate request status
                 */
                string[] stats = {hash, Universe.B64Decode(PIP), Universe.B64Decode(LIP), status};
                string sessID = string.Empty;
                for (int i = 0; i < 10; i++)
                {
                    sessID += Universe.ascii_lower[Universe.rng.Next(25)];
                    sessID += Universe.digits[Universe.rng.Next(9)];
                    sessID += Universe.ascii_upper[Universe.rng.Next(26)];

                }
                int existences = 0;
                foreach (System.Collections.Generic.KeyValuePair<string, string[]> pair in Universe.openRequests)
                {
                    if (pair.Value.Contains(hash))
                        existences++;
                    if (pair.Value[3] == status)
                        return $"{status}_request_already_open_for_this_account";
                }
                if (existences >= 2)
                    return "max_sessions_open_for_this_account";
                Universe.openRequests.Add(sessID, stats);
                Logg.printInfo(Universe.openRequests.Count.ToString() + " requests open on this server");
                return $"user_authentic&{sessID}";
            }
            else
            {
                return "user_not_authentic";
            }
        }


    }
}
