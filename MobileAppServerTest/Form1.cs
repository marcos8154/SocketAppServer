using SocketAppServerClient;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Text;

namespace MobileAppServerTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            txResult.ScrollBars = ScrollBars.Both;
            txPort.Focus();
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            try
            {
                treeView.Nodes.Clear();
                Client.Configure(txAddress.Text, int.Parse(txPort.Text), Encoding.UTF8, 1024000 * 10);
                Client client = new Client();

                RequestBody rb = RequestBody.Create("ServerInfoController", "FullServerInfo")
                    .AddParameter("authorization", Auth.Token ?? "");
                client.SendRequest(rb);

                var result = client.GetResult(typeof(ServerInfo));
                var serverInfo = (ServerInfo)result.Entity;

                lbDescription.Text = $"SocketAppServer, version {serverInfo.ServerVersion}, running on {serverInfo.MachineName}";

                foreach (var controllerInfo in serverInfo.ServerControllers)
                {
                    TreeNode node = new TreeNode(controllerInfo.ControllerName);
                    node.Tag = $"{controllerInfo.ControllerName}";

                    foreach (var action in controllerInfo.ControllerActions)
                    {
                        var child = new TreeNode(action);
                        child.Tag = $"{controllerInfo.ControllerName}:{action}";
                        node.Nodes.Add(child);
                    }

                    treeView.Nodes.Add(node);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unauthorized.") ||
                    ex.Message.Contains("Invalid or expired token"))
                {
                    Auth auth = new Auth();
                    auth.ShowDialog();
                    if (!string.IsNullOrEmpty(Auth.Token))
                        btConnect_Click(null, null);
                }
                else
                    MessageBox.Show("Error: \n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            CreateRequest();
        }

        private void treeView_DoubleClick(object sender, EventArgs e)
        {
            CreateRequest();
        }

        private void CreateRequest()
        {
            var node = treeView.SelectedNode;
            if (node == null)
                return;

            string[] info = node.Tag.ToString().Split(':');
            string controller = info[0];
            string action = (info.Length == 1
                ? ""
                : info[1]);

            var test = new TestRequest(controller, action);
            test.ShowDialog();

            var result = JsonConvert.SerializeObject(test.Result, Formatting.Indented);
            if (test.Result != null)
            {
                lbBytes.Visible = true;
                if (test.ServerResponse != null)
                    lbBytes.Text = $"Bytes used: {test.ServerResponse.ResponseLenght} ({test.ServerResponse.PercentUsage.ToString("N2")}% of server buffer)";
                else
                    lbBytes.Text = "";
                if (ckSaveToFile.Checked)
                {
                    if (!Directory.Exists(@".\ResponseData\"))
                        Directory.CreateDirectory(@".\ResponseData\");
                    File.WriteAllText($@".\ResponseData\{controller}-{action}.json", result);
                    Process.Start("notepad.exe", $@".\ResponseData\{controller}-{action}.json");
                }
                else
                    txResult.Text = result;
            }
        }
    }
}
