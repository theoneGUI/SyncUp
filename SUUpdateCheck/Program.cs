using System;
using System.Diagnostics;

namespace SU_UpdateCheck {
    class Program {

        public const string SERVER = "syncup.thatonetechcrew.net";
        public const int PORT = 8080;
        //static string rootDir = @"C:\Program Files\SyncUp\";
        static string rootDir = @"C:\Users\Aidan\Desktop\SyncUp\PRODUCTION\";
        static string[] filesToCheck = { @"DesktopApp\SyncUp.exe", @"SUFTP\SUFTP-recv.exe", @"SUFTP\SUFTP-send.exe", @"SUFTP\SUFTP-client.exe", @"SUFTP\SUFTP-server.exe", @"SUFTPListenerService\SUFTPListener.exe" };

        static void Main(string[] args)
        {
            checkForUpdates();

            //if (args[1].Equals("doUpdates"))
            //{
            //    doUpdates();
            //}
        }

        public static string getVer(string path)
        {
            var DesktopAppVer = FileVersionInfo.GetVersionInfo(path);
            return DesktopAppVer.FileVersion;
        }

        public static void checkForUpdates()
        {
            bool updatesAvailable = false;
            foreach (string assembly in filesToCheck)
            {
                string compare = Universe.getRequest($"https://{SERVER}:{PORT}/versions/check?assembly={assembly}");
                string current = getVer(rootDir + assembly);
                if (compare.Equals(current))
                {
                    Universe.everything[assembly] = "up_to_date";
                    //Console.WriteLine($"{assembly} is up to date ({current})");
                }
                else
                {
                    Universe.everything[assembly] = "update_available";
                    updatesAvailable = true;
                    //Console.WriteLine($"{assembly} is not up to date: {compare} available over {current}");
                }
            }
            if (updatesAvailable) Console.WriteLine("updates_available");
            else Console.WriteLine("all_up_to_date");
            
        }

        public static void doUpdates() { 
            foreach (System.Collections.Generic.KeyValuePair<string, string> file in Universe.everything)
            {
                if (file.Value.Equals("update_available"))
                {
                    Universe.downloadFile($"http://{SERVER}/current/{file.Key.Split(@"\")[1]}", (rootDir+file.Key));
                }
            }
            Universe.runCommand($@"{rootDir}DesktopApp\SyncUp.exe", "");
        }
    }
}
