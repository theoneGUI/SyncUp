using System;
using System.IO;
using System.Windows.Forms;

namespace SU_install_wizard_apollo {
    static class Program {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (isInstalled())
                Application.Run(new FixInstallForm());
            else
                Application.Run(new InstallForm());
        }

        public static bool isInstalled()
        {
            return Directory.Exists(@"C:\Program Files\SyncUp");
        }
    }

}
