using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

public class UserAuthMethods
{
    public static Dictionary<string, string> existingHashes = new Dictionary<string, string>();

    public static bool hashExists(string hash)
    {
        updateHashVar();
        Logg.printInfo($"Auth attempt made with hash: {hash}");
        return existingHashes.TryGetValue(hash, out _);
    }

    private static void updateHashVar()
    {
        string fileContents = File.ReadAllText("user_hashes.data");
        existingHashes = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContents);
    }
}
