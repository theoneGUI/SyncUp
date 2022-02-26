using System.IO;
using System;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace SU_install_wizard_apollo {
    public partial class KeyPopupForm : Form {
        public KeyPopupForm()
        {
            InitializeComponent();
        }

        private async void verifyKey_Click(object sender, EventArgs e)
        {
            this.verLabel.Text = "Verifying...";
            await Task.Run(runClick);
        }

        private void runClick()
        {
            string currentKey = this.textBox1.Text;
            Universe.keyToWrite = currentKey;
            if (Universe.getRequest($"https://syncup.thatonetechcrew.net:8080/auth/userauthho?hash={currentKey}").Equals("user_authentic"))
            {
                Universe.keyVerified = true;
                this.verLabel.Text = "Key verified!";
                this.textBox1.Enabled = false;
                this.verLabel.ForeColor = System.Drawing.Color.DarkGreen;
                this.verLabel.Visible = true;
                this.verifyKey.Enabled = false;
            }
            else
            {
                this.verLabel.Text = "Key not recognized. Please try again.";
                this.verLabel.ForeColor = System.Drawing.Color.DarkRed;
                this.verLabel.Visible = true;
            }
            Close();
        }
    }
}
