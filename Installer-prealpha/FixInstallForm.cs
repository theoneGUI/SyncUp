using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System;
using System.Net;
using System.IO.Compression;
using System.Threading.Tasks;

namespace SU_install_wizard_apollo {
    public partial class FixInstallForm : Form {
        public FixInstallForm()
        {
            InitializeComponent();
        }

        private void uninstall()
        {
            Universe.runCommand(Path.GetTempPath() + "\\SU-prealpha-install\\UNINSTALL.bat");
        }

        private void installFiles()
        {
            Universe.runCommand(Path.GetTempPath() + "\\SU-prealpha-install\\INSTALL.bat");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            this.status.Visible = true;
            this.button1.Enabled = false;
            this.button2.Enabled = false;
            this.progressBar1.Visible = true;
            this.progressBar1.Style = ProgressBarStyle.Marquee;
            this.progressBar1.Value = 1;
            this.actualStatus.Visible = true;
            this.actualStatus.Text = "Downloading installation files...";
            await Task.Run(uninstall);
            await Task.Run(Universe.downloadFiles);
            this.actualStatus.Text = "Reinstalling...";
            await Task.Run(installFiles);
            this.progressBar1.Value = 0;
            this.progressBar1.Visible = false;
            this.status.Visible = true;
            this.actualStatus.Text = "Changing key...";
            do
            {
                using (Form keyForm = new KeyPopupForm())
                {
                    keyForm.ShowDialog();
                }
            }
            while (!Universe.keyVerified);
            Universe.WriteToSettingsFile();
            this.actualStatus.Text = "Done! You can now exit.";
            this.actualStatus.Text = "Done!";

        }

        private async void button2_Click(object sender, EventArgs e)
        {
             var result = MessageBox.Show("Are you sure you want to uninstall SyncUp? (This removes all configuration files, so if you try to reinstall later, you'll need your key on hand again.",
                "Uninstall SyncUp?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);

            if (result.Equals(DialogResult.Yes))
            {
                this.status.Visible = true;
                this.button1.Enabled = false;
                this.button2.Enabled = false;
                this.button3.Enabled = false;
                this.actualStatus.Visible = true;
                this.actualStatus.Text = "Uninstalling...";
                await Task.Run(uninstall);
                this.actualStatus.Text = "SyncUp uninstalled!";
            }

            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.status.Visible = true;
            this.actualStatus.Text = "Changing key...";
            using (Form keyForm = new KeyPopupForm())
            {
                keyForm.ShowDialog();
            }
            Universe.WriteToSettingsFile();
            this.actualStatus.Text = "Done! You can now exit.";
        }
    }
}
