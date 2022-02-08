using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncUpAPI.Controllers {
    [ApiController]
    [Route("/auth/qss")]
    public class QuerySessionState : ControllerBase {
        [HttpGet]
        public string Get(string hash, string sessID)
        {
            int occurences = 0;
            string pair = string.Empty;
            string thisStatus;
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    foreach (System.Collections.Generic.KeyValuePair<string, string[]> entry in Universe.openRequests)
                    {
                        if (entry.Key.Equals(sessID))
                        {
                            thisStatus = entry.Value[3];
                        }
                        if (entry.Value.Contains(hash) && !entry.Key.Equals(sessID) && !entry.Value.Contains(Universe.openRequests[sessID][3]))
                        {
                            pair = entry.Key;
                        }
                        if (entry.Value.Contains(hash))
                            occurences++;
                    }
                }
                occurences = (int)Math.Floor(((double)occurences) / 2);
                if (occurences != 2 || pair.Equals(string.Empty)) return "match_does_not_exist";
                else if (occurences != 2 && pair.Equals(string.Empty)) return "match_does_not_exist";
                else
                {
                    if (!Universe.readyToDelete.ContainsKey(hash))
                        Universe.readyToDelete.Add(hash, new List<string>());
                    Universe.readyToDelete[hash].Add(sessID);
                    return $"match&{Universe.B64Encode(Universe.openRequests[pair][1])}&{Universe.B64Encode(Universe.openRequests[pair][2])}";
                }
            }
            catch (KeyNotFoundException)
            {
                return "match_does_not_exist";
            }
        }

    }
}
