using System.Collections.Generic;
using System.IO;
using System.Net;

public static class Universe
{
    //declare variables for global use
    public static IDictionary<string, string> everything = new Dictionary<string, string>(); 
    public static int errors = 0;
    public static int totalFiles;
    public static int inProgress = 1;

    public static string getRequest(string url)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.AutomaticDecompression = DecompressionMethods.GZip;
        try
        {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
        catch (System.Net.WebException)
        {
            return "SERVER_UNRESPONSIVE";
        }
    }
    public static string ToJSON(List<string> input)
    {
        return System.Text.Json.JsonSerializer.Serialize(input);
    }
    public static List<string> getLocalIP()
    {
        List<string> ips = new List<string>();
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                ips.Add(ip.ToString());
            }
        }
        return ips;
    }

    public static string getPublicIP()
    {
        string publicIP = getRequest("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
        // var publicIPstring = IPAddress.Parse(publicIP);
        // return publicIPstring.ToString();
        return publicIP;
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
