using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace SyncUpAPI {
    public class Universe {
        public static Dictionary<string, string[]> openRequests = new Dictionary<string, string[]>();
        public static Dictionary<string, List<string>> readyToDelete = new Dictionary<string, List<string>>();
        public static Dictionary<string, string> errors = new Dictionary<string, string>();
        public static bool END_OF_WORLD = false;

        public const string ascii_upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string ascii_lower = "abcdefghijklmnopqrstuvwxyz";
        public const string digits = "0123456789";

        public readonly static Random rng = new Random();

        public static void Tick()
        {
            int removed = 0;
            foreach (System.Collections.Generic.KeyValuePair<string, List<string>> entry in readyToDelete)
            {
                if (entry.Value.Count == 2) {
                    openRequests.Remove(entry.Value[0]);
                    openRequests.Remove(entry.Value[1]);
                    readyToDelete.Remove(entry.Key);
                    removed++;
                }
            }
            if (removed > 0)
                Logg.printInfo($"Removed {removed} matchmaking keys");
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

        public static void DoQSSTick()
        {
            Thread t = new Thread(DoErrorTick);
            t.Start();
            while (true)
            {
                Tick();
                Thread.Sleep(6000);
                if (END_OF_WORLD)
                {
                    Logg.printInfo("QSS ticker thread killed");
                    t.Join();
                    Logg.printInfo("Error ticker thread killed");
                    break;
                }
            }
        }

        public static void DoErrorTick()
        {
            while (true)
            {
                foreach (System.Collections.Generic.KeyValuePair<string, string> pair in errors)
                {
                    if (pair.Value.Equals("complete"))
                    {
                        errors.Remove(pair.Key);
                    }
                }
                for (int i = 0; i < 1000; i++)
                {
                    if (END_OF_WORLD)
                        break;
                    else
                        Thread.Sleep(3600);
                }
                if (END_OF_WORLD)
                    break;
            }
        }
        public static string VersionLookup(string assembly)
        {
            string[] versionFileContents = File.ReadAllLines("versions.data");
            string result = "assembly_not_found";
            foreach (string line in versionFileContents)
            {
                string[] betterLine = line.Split("=");
                if (betterLine[0].Equals(assembly))
                {
                    return betterLine[1];
                }
            }

            return result;
        }

    }
}
