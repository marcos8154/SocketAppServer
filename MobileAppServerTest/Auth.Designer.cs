namespace MobileAppServerTest
{
    partial class Auth
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txUser = new System.Windows.Forms.TextBox();
            this.txPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btAuthenticate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "This server requires authentication";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "User\r\n";
            // 
            // txUser
            // 
            this.txUser.Location = new System.Drawing.Point(12, 67);
            this.txUser.Name = "txUser";
            this.txUser.Size = new System.Drawing.Size(272, 20);
            this.txUser.TabIndex = 1;
            this.txUser.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txUser_KeyDown);
            // 
            // txPassword
            // 
            this.txPassword.Location = new System.Drawing.Point(12, 109);
            this.txPassword.Name = "txPassword";
            this.txPassword.PasswordChar = '*';
            this.txPassword.Size = new System.Drawing.Size(272, 20);
            this.txPassword.TabIndex = 2;
            this.txPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txPassword_KeyDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 93);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Password";
            // 
            // btAuthenticate
            // 
            this.btAuthenticate.Location = new System.Drawing.Point(12, 146);
            this.btAuthenticate.Name = "btAuthenticate";
            this.btAuthenticate.Size = new System.Drawing.Size(272, 23);
            this.btAuthenticate.TabIndex = 3;
            this.btAuthenticate.Text = "Authenticate";
            this.btAuthenticate.UseVisualStyleBackColor = true;
            this.btAuthenticate.Click += new System.EventHandler(this.btAuthenticate_Click);
            // 
            // Auth
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(296, 186);
            this.Controls.Add(this.btAuthenticate);
            this.Controls.Add(this.txPassword);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txUser);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Auth";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Auth";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txUser;
        private System.Windows.Forms.TextBox txPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btAuthenticate;
    }
}