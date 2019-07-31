using MobileAppServerClient;
using Newtonsoft.Json;
using System;
using System.Windows.Forms;

namespace MobileAppServerTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            try
            {
                treeView.Nodes.Clear();
                Client.Configure(txAddress.Text, int.Parse(txPort.Text));
                Client client = new Client();

                RequestBody rb = RequestBody.Create("ServerInfoController", "FullServerInfo");
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

            if (test.Result != null)
                txResult.Text = JsonConvert.SerializeObject(test.Result, Formatting.Indented);
        }
    }
}
