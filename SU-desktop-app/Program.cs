using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Web;
using System.Threading;

namespace SyncUp
{
    class Program
    {
        static void Main(string[] args)
        {
            Universe.UpdateCheck();

            List<string> LIPs = Universe.getLocalIP();
            string PIP = Universe.getPublicIP();
            string status = "SEND";
            string SERVER = Universe.getSettings()["SERVER"];
            int PORT = int.Parse(Universe.getSettings()["PORT"]);
            string USERID = Universe.getSettings()["USERHASH"];

            // make sure the user registered with this program is a real user
            string userAuthResp = Universe.getRequest($"https://{SERVER}:{PORT}/auth/userauthho?hash={USERID}");
            if (userAuthResp.Contains("user_authentic"))
                Terminal.printInfo("User authenticated; continuing");
            else if (userAuthResp.Equals("SERVER_UNRESPONSIVE")) {
                Terminal.printError("Program authentication server could not be found\nCANNOT CONTINUE!!!\n");
                Terminal.exitOnKeyPress();
                Environment.Exit(404);
            }
            else
            {
                Terminal.printError("USER NOT AUTHENTIC... EXITTING!");
                Terminal.exitOnKeyPress();
                Environment.Exit(1);
            }



            //Start a timer to see how long the entire operation will take
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //Set up main variables and locate user home folder/temporary file folder as reported by PATH
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string tempFolder = Path.GetTempPath();

            try
            {
                //Try to open the old hash file (if it exists) and read data as json and deserialize it into itertable format
                string[] stream = File.ReadAllLines(tempFolder + "\\hashes.metric");
                var oldHashes = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream[0]); //<- may be able to save highly changed files on line 2 or something to provide "quick search" vs "full search" options for user
                stream = null; //clear up some memory since string[] stream has already been processed (no longer needed)
                GC.Collect();
                string url = $"https://{SERVER}:{PORT}/auth/userauth?hash={USERID}&PIP={HttpUtility.HtmlEncode(Universe.B64Encode(PIP))}&LIP={HttpUtility.HtmlEncode(Universe.B64Encode(Universe.ToJSON(LIPs)))}&status={status}";
                string response = Universe.getRequest(url);
                //Calculate new hashes and save
                getHashes(home, tempFolder, false);
                //prep variables for change comparison
                int passes = 0;
                int fails = 0;
                Dictionary<String, String> changedFiles = new Dictionary<String, String>();
                //iterate over all file hashes (new and old) to see what's new
                foreach (System.Collections.Generic.KeyValuePair<string, string> hash in oldHashes)
                {
                    try
                    {
                        //if new hash matches old hash then don't worry about it
                        if (Universe.everything[hash.Key].Equals(oldHashes[hash.Key]))
                            passes++;
                        //if new hash doesn't match old one, then add it to list of files to be further processed
                        else
                        {
                            changedFiles.Add(hash.Key, hash.Value);
                            fails++;
                        }
                    }
                    //file may have been deleted so catch an exception in the case that a key value doesn't exist
                    catch (Exception e)
                    {
                        Terminal.printError(e.Message); //tell user and count the fail
                        fails++;
                        Universe.errors++;
                    }
                }
                //WAIT FOR ALL CLEAR FROM API!!!!!!!!!!!!!!!:
                string SESSION_ID = "";
                Terminal.printInfo("Waiting for recipient PC...");
                try
                {
                    SESSION_ID= response.Split("&")[1];
                }
                catch (Exception)
                {
                    Terminal.printError("There was a fatal error authenticating this session.");
                    Terminal.printErrorDetails("Close this window and start SyncUp again on both PCs you want to tranfser between");
                    Terminal.exitOnKeyPress();
                }
                string[] parameters = { };
                while (true)
                {
                    string waiting = $"https://{SERVER}:{PORT}/auth/qss?hash={USERID}&sessID={SESSION_ID}";
                    string resp = Universe.getRequest(waiting);
                    if (resp.Contains("match&"))
                    {
                        parameters = resp.Split("&");
                        break;
                    }
                    Thread.Sleep(500);
                }
                Universe.getRequest($"https://{SERVER}:{PORT}/error/errorCheck?sessID={SESSION_ID}&status=SET&data=no_error");
                string TARGET_IP = SERVER;
                //If public IPs are in common, find the interface that both PCs are on
                List<string> remoteLIPs = System.Text.Json.JsonSerializer.Deserialize<List<string>>(Universe.B64Decode(parameters[2]));
                string RemotePIP = Universe.B64Decode(parameters[1]);
                List<string[]> LIPsDigested = new List<string[]>();
                bool BREAK_ALL = false;
                foreach (string i in LIPs) { LIPsDigested.Add(i.Split('.')); }
                if (PIP.Equals(RemotePIP))
                {
                    foreach (string[] LIP in LIPsDigested)
                    {
                        string subnet = LIP[0] + "." + LIP[1] + "." + LIP[2] + ".0";
                        foreach (string RIP in remoteLIPs)
                        {
                            string[] t = RIP.Split('.');
                            string rSubnet = t[0] + "." + t[1] + "." + t[2] + ".0";
                            if (subnet.Equals(rSubnet))
                            {
                                Terminal.printInfo($"FOUND IP ADDRESS MATCH {RIP}");
                                TARGET_IP = RIP;
                                BREAK_ALL = true;
                                break;
                            }
                        }
                        if (BREAK_ALL) break;
                    }
                }

                //Update hashes file
                Console.Write("Updating hash file... ");
                string[] lines = { System.Text.Json.JsonSerializer.Serialize(Universe.everything) };
                File.WriteAllLines(tempFolder + "\\hashes.metric", lines);
                Terminal.printInfo("[DONE]");
                //Tell how many things worked and didn't
                Terminal.printInfo($"A total of {Universe.errors} errors occured while processing all data");
                Terminal.printInfo($"{passes} files remain unchanged on disk");
                Terminal.printWarning($"{fails} files changed\n");
                Terminal.printInfo($"Preparing to transfer {changedFiles.Count} changed files...");

                changedFiles.Add("TARGET_IP_ADDRESS", TARGET_IP);                 //get the real target IP and put it here
                Terminal.printInfo($"!!!!!!!!!!!USING IP ADDRESS: {TARGET_IP} for transfer");
                string cmdLineArg = System.Text.Json.JsonSerializer.Serialize(changedFiles);
                if (!TARGET_IP.Equals(SERVER))
                {
                    Terminal.printInfo("Starting SUFTP over LAN...");
                    string transferResponse = startOnLAN($"{cmdLineArg}");
                    if (transferResponse.Contains("LAN SERVER DID NOT RESPOND!"))
                    {
                        Universe.getRequest($"https://{SERVER}:{PORT}/error/errorCheck?sessID={SESSION_ID}&status=SET&data=use_inet");
                        Terminal.printWarning("Network error occurred");
                        Terminal.printWarning("Nothing fatal happened, but this might take a little longer...");
                        Terminal.printInfo("Starting SUFTP over internet...");
                        Terminal.printInfo(startOverInet($"{cmdLineArg}"));
                    }
                }
                else
                {
                    Terminal.printInfo("Starting SUFTP over internet...");
                    Terminal.printInfo(startOverInet($"{cmdLineArg}"));
                }
                Console.Write("\n\n");
                Universe.getRequest($"https://{SERVER}:{PORT}/error/errorCheck?sessID={SESSION_ID}&status=SET&data=complete");
            }
            //Andddddddd if the hash file isn't saved then create a whole new one and exit
            catch (System.IO.FileNotFoundException)
            {
                Terminal.printWarning("HASH FILE DOES NOT EXIST!\nCreating new file... (this may take a bit)");
                try { getHashes(home, tempFolder, true); } //create file
                //if something goes wrong tell and quit
                catch (Exception e)
                {
                    Terminal.printError("Sorry... we ran into a problem while parsing data: " + e.Message+"\nPlease report this problem and error message to the developer.");
                    Terminal.printInfo("You can submit a bug report on SyncUp's GitHub or you can visit www.thatonetechcrew/communications and submit the issue there.\nPlease include the exact error message above.");
                }
            }

            //Timing
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Terminal.printInfo("RunTime " + elapsedTime);
            Terminal.exitOnKeyPress();

        }
            static void getHashes(string home, string tempFolder, bool flushToFile)
        {
            try
            {
                //Count total number of files in target directory for stats during execution
                Universe.totalFiles = countFiles(home + "\\Desktop");  //original
                //Print some diagnostic data
                Terminal.printInfo($"Using variables:\nHOME PATH: {home}\nTEMP PATH: {tempFolder}\nITEMS IN DESKTOP: {Universe.totalFiles}\n");
                listDirRecursive(home + "\\Desktop");                  //original


                //if hashes are supposed to be flushed to file, then do that
                if (flushToFile) { 
                    string[] lines = { System.Text.Json.JsonSerializer.Serialize(Universe.everything) };
                    File.WriteAllLines(tempFolder + "\\hashes.metric", lines);
                }
                Terminal.printInfo($"\n{Universe.errors} permission errors occured while processing the data");
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                string Dtop = (home + "\\OneDrive\\Desktop");
                Universe.totalFiles = countFiles(Dtop);
                Terminal.printInfo($"Using variables:\nHOME PATH: {home}\nTEMP PATH: {tempFolder}\nITEMS IN DESKTOP: {Universe.totalFiles}");
                listDirRecursive(Dtop);
                if (flushToFile) {
                    string[] lines = { System.Text.Json.JsonSerializer.Serialize(Universe.everything) };
                    File.WriteAllLines(tempFolder + "\\hashes.metric", lines);
                }
                Terminal.printWarning($"\n{Universe.errors} errors occured while processing the data");
            }
            catch (Exception e)
            {
                Terminal.printError(e.Message);
                Terminal.printErrorDetails(e.StackTrace);
                Terminal.exitOnKeyPress();
            }
        }
        static void listDirRecursive(String path)
        {
            try
            {
                string[] dirFiles = Directory.GetFiles(path);
                string[] dirDirs = Directory.GetDirectories(path);
                foreach (string file in dirFiles)
                {
                    Terminal.drawPB(Universe.totalFiles, Universe.inProgress);
                    string hash = hashFile(file);
                    Universe.everything[file] = hash;
                    Universe.inProgress++;
                }
                foreach (string dir in dirDirs)
                {
                    listDirRecursive((dir));
                }
            }
            catch (System.UnauthorizedAccessException)
            {
                Universe.errors++;
            }
        }
        static string hashFile(String fp)
        {
            try
            {
                //create hashing object
                using (var hash = SHA256.Create())
                {
                    //open file read stream
                    using (var stream = File.OpenRead(fp))
                    {
                        //and hash it by adding all file contents to hashing object
                        byte[] bytes = hash.ComputeHash(stream);
                        StringBuilder builder = new StringBuilder();
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            builder.Append(bytes[i].ToString("x2"));
                        }
                        return builder.ToString();
                    }
                }
            }
            //handle things that might go wrong
            catch (System.IO.InvalidDataException)
            {
                return "NOT A FILE";
            }
            catch (System.UnauthorizedAccessException)
            {
                return "I do not have access to that file/directory";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        static int countFiles(String path)
        {
            //init counter
            int files = 0;
            try
            {
                //add number of files to total
                files += Directory.GetFiles(path).Length;
                foreach (string dir in Directory.GetDirectories(path))
                {
                    //for every directory within directory, open it and count files within it recursively
                    files += countFiles((dir));
                }
            }
            //if I don't have access to the folder then skip it and mark it down as an error
            catch (System.UnauthorizedAccessException)
            {
                Universe.errors++;
            }
            return files;
        }
        public static string startOnLAN(string arguments)
        {
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.Arguments = $@"client {arguments}";
            p.StartInfo.FileName = "\"C:\\Program Files\\SyncUp\\SUFTP-all.exe\"";
            Terminal.printInfo(p.StartInfo.FileName+" "+p.StartInfo.Arguments); ////////////////////////////////////////////////////////////////////////////
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string response = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return response;
        }
        public static string startOverInet(string arguments)
        {
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.Arguments = $@"send {arguments}";
            p.StartInfo.FileName = "\"C:\\Program Files\\SyncUp\\SUFTP-all.exe\"";
            Terminal.printInfo(p.StartInfo.FileName + " " + p.StartInfo.Arguments); ////////////////////////////////////////////////////////////////////////////
            p.Start();

            string response = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return response;
        }
    }
}
