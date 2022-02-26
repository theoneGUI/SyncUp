using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System;
using System.Threading;

public static class Universe
{
    //declare variables for global use
    public static IDictionary<string, string> everything = new Dictionary<string, string>(); 
    public static int errors = 0;
    public static int totalFiles;
    public static int inProgress = 1;
    public static string cwd = Directory.GetCurrentDirectory();
    public static string STDOUT = "";
    public static string STDERR = "";


    /*public static string getRequest(string url)
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
    }*/

    public static string getRequest(string url)
    {
        string cmd = $"\"{url}\" -ks";
        Process p = new Process();
        // Redirect the output stream of the child process.
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.Arguments = $@"{cmd}";
        p.StartInfo.FileName = "\"C:\\Program Files\\SyncUp\\curl\\curl.exe\"";
        p.Start();
        // Do not wait for the child process to exit before
        // reading to the end of its redirected stream.
        // p.WaitForExit();
        // Read the output stream first and then wait.
        string response = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        return response;
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

    public static Dictionary<string, string> getSettings()
    {
        Dictionary<string, string> settingsInFile = new Dictionary<string, string>();
        try
        {
            string[] fileStream = File.ReadAllLines("C:\\Program Files\\SyncUp\\settings.config");
            foreach (string preference in fileStream)
            {
                if (preference.Contains("#"))
                    continue;
                string[] tmp = preference.Split('=');
                settingsInFile[tmp[0]] = tmp[1];
            }
        }
        catch (Exception)
        {
            Terminal.printError("SETTINGS FILE NOT FOUND!");
            Terminal.printError("QUITTING!");
            Terminal.exitOnKeyPress();
        }
        return settingsInFile;
    }
    public static string runCommand(string exe, string parameters)
    {
        // Redirect the output stream of the child process.
        Process p = new Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.CreateNoWindow = true;

        p.StartInfo.Arguments = $"{parameters}";
        p.StartInfo.FileName = $"{exe}";
        p.Start();
        string response = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        return response;
    }
    public static void runDoUpdates() { runCommand(@"C:\Program Files\SyncUp\SU-UpdateCheck.exe", "doUpdates");  }
    public static void UpdateCheck()
    {
        string report = runCommand(@"C:\Program Files\SyncUp\SU-UpdateCheck.exe", "");
        Terminal.printInfo("Update check result: " + report);
        if (!report.Equals("all_up_to_date"))
        {
            Terminal.printWarning("Updates required.");
            Terminal.printWarning("I'm going to quit and install updates... this shouldn't take long");
            Thread updateBackground = new Thread(runDoUpdates);
            updateBackground.Start();
            Environment.Exit(66);
        }
        else
        {
            Terminal.printInfo("SyncUp is up to date!");
        }
    }
}
