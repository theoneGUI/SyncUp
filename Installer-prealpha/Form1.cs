using System.IO;
using System;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace SU_install_wizard_apollo {
    public partial class InstallForm : Form {
        public InstallForm()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, System.EventArgs e)
        {
            if (!Universe.keyVerified)
            {
                using (Form keyForm = new KeyPopupForm())
                {
                    keyForm.ShowDialog();
                }
            }
            else
            {
                this.button1.Enabled = false;
                this.statusLabel.Visible = true;
                this.statusLabel.Text = "Downloading installation files...";
                this.label3.Visible = true;
                this.progressBar1.Visible = true;
                this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
                this.progressBar1.Value = 1;
                await Task.Run(downloadFiles);
                this.statusLabel.Text = "Installing SyncUp...";
                await Task.Run(installFiles);
                this.progressBar1.Value = 0;
                this.statusLabel.Text = "Done!";
                this.progressBar1.Visible = false;
            }

        }

        private void downloadFiles()
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
                Universe.runCommand($"del {Path.GetTempPath()}\\SU-prealpha-install /q /f /s");
                Directory.CreateDirectory(Path.GetTempPath() + "\\SU-prealpha-install");
                ZipFile.ExtractToDirectory(Path.GetTempPath() + "\\SU-prealpha-build.zip", Path.GetTempPath() + "\\SU-prealpha-install");
            }
        }

        private void installFiles()
        {
            Universe.runCommand(Path.GetTempPath() + "\\SU-prealpha-install\\INSTALL.bat");
            Universe.WriteToSettingsFile();
        }

        private void checkBox1_CheckedChanged(object sender, System.EventArgs e)
        {
            if (this.checkBox1.Checked)
            {
                this.button1.Enabled = true;
                using (Form keyForm = new KeyPopupForm())
                {
                    keyForm.ShowDialog();
                }

            }
            else
                this.button1.Enabled = false;
        }

    }
}
