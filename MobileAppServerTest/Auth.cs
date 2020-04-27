using SocketAppServerClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MobileAppServerTest
{
    public partial class Auth : Form
    {
        public static string Token { get; private set; }

        private static string PreviousUser { get; set; }
        private static string PreviousPasswd { get; set; }

        public Auth()
        {
            InitializeComponent();

            if (Token == null)
                Token = "";
            txUser.Text = PreviousUser;
            txPassword.Text = PreviousPasswd;
        }

        private void txUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txPassword.Focus();
        }

        private void txPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Authenticate();
        }

        private void Authenticate()
        {
            try
            {
                Client c = new Client();
                RequestBody rb = RequestBody.Create("AuthorizationController",
                    "Authorize")
                    .AddParameter("user", txUser.Text)
                    .AddParameter("password", txPassword.Text);
                c.SendRequest(rb);
                OperationResult result = c.GetResult();
                Token = result.Entity.ToString();

                PreviousUser = txUser.Text;
                PreviousPasswd = txPassword.Text;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btAuthenticate_Click(object sender, EventArgs e)
        {
            Authenticate();
        }
    }
}
