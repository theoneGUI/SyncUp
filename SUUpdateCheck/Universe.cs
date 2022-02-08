using System.Collections.Generic;
using System.Diagnostics;


public static class Universe {
    //declare variables for global use
    public static IDictionary<string, string> everything = new Dictionary<string, string>();
    public static int errors = 0;
    public static int totalFiles;
    public static int inProgress = 1;
    public static string STDOUT = "";
    public static string STDERR = "";

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

    public static string downloadFile(string url, string outputFile)
    {
        string cmd = $"\"{url}\" -ks";
        Process p = new Process();
        // Redirect the output stream of the child process.
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.Arguments = $@"{cmd} -o {outputFile}";
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
    public static string runCommand(string exe, string parameters)
    {
        // Redirect the output stream of the child process.
        Process p = new Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.CreateNoWindow = true;

        p.StartInfo.Arguments = $"/C \"{exe}\" {parameters}";
        p.StartInfo.FileName = "cmd.exe";
        p.Start();
        string response = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        return response;
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
