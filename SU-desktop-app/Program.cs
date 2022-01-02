using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Web;

namespace SyncUp
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> LIPs = Universe.getLocalIP();
            string PIP = Universe.getPublicIP();
            string status = "SEND";
            
            // make sure the user registered with this program is a real user
            const string USERID = "ALPHA_VERSION_PLACEBO_HASH";
            string url = $"https://localhost:8080/auth/userauth?hash={USERID}&PIP={HttpUtility.HtmlEncode(Universe.B64Encode(PIP))}&LIP={HttpUtility.HtmlEncode(Universe.B64Encode(Universe.ToJSON(LIPs)))}&status={status}";
            string response = Universe.getRequest(url);
            if (response.Equals("user_authentic"))
                Console.WriteLine("User authenticated; continuing");
            else if (response.Equals("SERVER_UNRESPONSIVE")) {
                Console.WriteLine("ERROR! :: program authentication server could not be found\nCANNOT CONTINUE!!!\n");
                Terminal.exitOnKeyPress();
                Environment.Exit(404);
            }
            else
            {
                Console.WriteLine("USER NOT AUTHENTIC... EXITTING!");
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

                //Calculate new hashes and save
                getHashes(home, tempFolder, false);
                //prep variables for change comparison
                int passes = 0;
                int fails = 0;
                List<String> changedFiles = new List<String>();
                //iterate over all file hashes (new and old) to see what's new
                foreach (System.Collections.Generic.KeyValuePair<string, string> hash in oldHashes)
                {
                    try
                    {
                        //if new hash matches old hash then don't worry about it
                        if (Universe.everything[hash.Key] == oldHashes[hash.Key])
                            passes++;
                        //if new hash doesn't match old one, then add it to list of files to be further processed
                        else
                        {
                            changedFiles.Add(hash.Key);
                            fails++;
                        }
                    }
                    //file may have been deleted so catch an exception in the case that a key value doesn't exist
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message); //tell user and count the fail
                        fails++;
                        Universe.errors++;
                    }
                }
                //Update hashes file
                Console.Write("Updating hash file... ");
                string[] lines = { System.Text.Json.JsonSerializer.Serialize(Universe.everything) };
                File.WriteAllLines(tempFolder + "\\hashes.metric", lines);
                Console.WriteLine("[DONE]");
                //Tell how many things worked and didn't
                Console.WriteLine($"\n\nA total of {Universe.errors} errors occured while processing all data");
                Console.WriteLine($"{passes} files remain unchanged on disk\n{fails} files changed\n");

                foreach (String i in changedFiles)
                {
                    Console.WriteLine($"{i} changed on disk, preparing to transfer");
                }
                Console.Write("\n\n");
            }
            //Andddddddd if the hash file isn't saved then create a whole new one and exit
            catch (System.IO.FileNotFoundException)
            {
                Console.WriteLine("HASH FILE DOES NOT EXIST!\nCreating new file... (this may take a bit)");
                try { getHashes(home, tempFolder, true); } //create file
                //if something goes wrong tell and quit
                catch (Exception e)
                {
                    Console.WriteLine("Sorry... we ran into a problem while parsing data: " + e.Message+"\nPlease report this problem and error message to the developer.");
                }
            }

            //Timing
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            Terminal.exitOnKeyPress();

        }
        static void getHashes(string home, string tempFolder, bool flushToFile)
        {
            try
            {
                //Count total number of files in target directory for stats during execution
                Universe.totalFiles = countFiles(home + "\\Desktop");
                //Print some diagnostic data
                Console.WriteLine($"Using variables:\nHOME PATH: {home}\nTEMP PATH: {tempFolder}\nITEMS IN DESKTOP: {Universe.totalFiles}\n");
                listDirRecursive(home + "\\Desktop");
                //if hashes are supposed to be flushed to file, then do that
                if (flushToFile) { 
                    string[] lines = { System.Text.Json.JsonSerializer.Serialize(Universe.everything) };
                    File.WriteAllLines(tempFolder + "\\hashes.metric", lines);
                }
                Console.WriteLine($"\n{Universe.errors} permission errors occured while processing the data");
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                string Dtop = (home + "\\OneDrive\\Desktop");
                Universe.totalFiles = countFiles(Dtop);
                Console.WriteLine($"Using variables:\nHOME PATH: {home}\nTEMP PATH: {tempFolder}\nITEMS IN DESKTOP: {Universe.totalFiles}\n\n");
                listDirRecursive(Dtop);
                if (flushToFile) {
                    string[] lines = { System.Text.Json.JsonSerializer.Serialize(Universe.everything) };
                    File.WriteAllLines(tempFolder + "\\hashes.metric", lines);
                }
                Console.WriteLine($"\n{Universe.errors} errors occured while processing the data");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
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
    }


}
