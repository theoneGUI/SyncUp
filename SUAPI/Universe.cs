using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;

namespace SyncUpAPI {
    public class Universe {
        public static Dictionary<string, string[]> openRequests = new Dictionary<string, string[]>();

        public const string ascii_upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string ascii_lower = "abcdefghijklmnopqrstuvwxyz";
        public const string digits = "0123456789";

        public static Random rng = new Random();

        public static void Tick()
        {
            foreach (System.Collections.Generic.KeyValuePair<string, string[]> entry in openRequests)
            {
                foreach (System.Collections.Generic.KeyValuePair<string, string[]> permutation in openRequests)
                {
                    if (entry.Key.Equals(permutation.Key))
                    {
                        continue;
                    }
                    else
                    {
                        if (entry.Value.Equals(permutation.Value)) {
                            openRequests.Remove(entry.Key);
                            openRequests.Remove(permutation.Key);
                        }
                        else if (entry.Value[0].Equals(permutation.Value[0]) && !entry.Value[3].Equals(permutation.Value[3]))
                        {
                            //TODO Notify both recipients of action and begin file transfer over appropriate channel
                            openRequests.Remove(entry.Key);
                            openRequests.Remove(permutation.Key);
                        }
                    }
                }
            }
        }
        public static string B64Encode(string str)
        {
            var convert = System.Text.Encoding.UTF8.GetBytes(str);
            return System.Convert.ToBase64String(convert);
        }

        public static string B64Decode(string str)
        {
            var convert = System.Convert.FromBase64String(str);
            return System.Text.Encoding.UTF8.GetString(convert);
        }
    }
}
