
namespace SU_install_wizard_apollo {
    partial class KeyPopupForm {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KeyPopupForm));
            this.label4 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.verifyKey = new System.Windows.Forms.Button();
            this.verLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 23);
            this.label4.TabIndex = 9;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 41);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(319, 23);
            this.textBox1.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 23);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(319, 15);
            this.label5.TabIndex = 11;
            this.label5.Text = "Copy and paste the key included in your release email here:";
            // 
            // verifyKey
            // 
            this.verifyKey.Location = new System.Drawing.Point(12, 70);
            this.verifyKey.Name = "verifyKey";
            this.verifyKey.Size = new System.Drawing.Size(75, 23);
            this.verifyKey.TabIndex = 12;
            this.verifyKey.Text = "Verify";
            this.verifyKey.UseVisualStyleBackColor = true;
            this.verifyKey.Click += new System.EventHandler(this.verifyKey_Click);
            // 
            // verLabel
            // 
            this.verLabel.AutoSize = true;
            this.verLabel.ForeColor = System.Drawing.Color.SeaGreen;
            this.verLabel.Location = new System.Drawing.Point(146, 74);
            this.verLabel.Name = "verLabel";
            this.verLabel.Size = new System.Drawing.Size(71, 15);
            this.verLabel.TabIndex = 13;
            this.verLabel.Text = "Key verified!";
            this.verLabel.Visible = false;
            // 
            // KeyPopupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(347, 117);
            this.Controls.Add(this.verLabel);
            this.Controls.Add(this.verifyKey);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "KeyPopupForm";
            this.Text = "SyncUp Key Verify";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button verifyKey;
        private System.Windows.Forms.Label verLabel;
    }
}

