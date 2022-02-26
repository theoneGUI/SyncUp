using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public static class Universe {

    public static bool keyVerified = false;
    public static string keyToWrite = string.Empty;

    public static string getRequest(string url)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.AutomaticDecompression = DecompressionMethods.GZip;
        try
        {
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
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

    public static void WriteToSettingsFile()
    {
        using (StreamWriter settings = File.CreateText(@"C:\Program Files\SyncUp\settings.config"))
        {
            settings.WriteLine("SERVER=syncup.thatonetechcrew.net");
            settings.WriteLine("PORT=8080");
            settings.WriteLine($"USERHASH={keyToWrite}");
        }
    }
    public static void downloadFiles()
    {
        using (var client = new WebClient())
        {
            client.DownloadFile("https://syncup.thatonetechcrew.net/current/SU-prealpha-build.zip", (Path.GetTempPath() + "\\SU-prealpha-build.zip"));
        }
        Directory.CreateDirectory(Path.GetTempPath() + "\\SU-prealpha-install");
        try
        {
            ZipFile.ExtractToDirectory(Path.GetTempPath() + "\\SU-prealpha-build.zip", Path.GetTempPath() + "\\SU-prealpha-install");
        }
        catch (Exception)
        {
            runCommand($"del {Path.GetTempPath()}\\SU-prealpha-install /q /f /s");
            Directory.CreateDirectory(Path.GetTempPath() + "\\SU-prealpha-install");
            ZipFile.ExtractToDirectory(Path.GetTempPath() + "\\SU-prealpha-build.zip", Path.GetTempPath() + "\\SU-prealpha-install");
        }
    }

    private static bool AcceptAllCertifications(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    public static string runCommand(string parameters)
    {
        // Redirect the output stream of the child process.
        Process p = new Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.CreateNoWindow = true;

        p.StartInfo.Arguments = $"/c {parameters}";
        p.StartInfo.FileName = "cmd.exe";
        p.Start();
        string response = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        return response;
    }
    public static string runCommand(string parameters, bool hideTerminal)
    {
        // Redirect the output stream of the child process.
        Process p = new Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.CreateNoWindow = hideTerminal;

        p.StartInfo.Arguments = $"/c {parameters}";
        p.StartInfo.FileName = "cmd.exe";
        p.Start();
        string response = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        return response;
    }


}
