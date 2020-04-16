using MobileAppServerClient;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MobileAppServerTest
{
    public partial class TestRequest : Form
    {
        public OperationResult Result { get; private set; }
        public ServerResponse ServerResponse { get; private set; }
        public TestRequest(string controller, string action)
        {
            InitializeComponent();

            if (!LoadFromCache(controller, action))
            {
                txController.Text = controller;
                txAction.Text = action;
            }
        }

        private bool LoadFromCache(string controller, string action)
        {
            string dir = @".\RequestCaches\";
            string file = $@"{dir}\{controller}.{action}.json";

            if (!File.Exists(file))
                return false;

            string json = File.ReadAllText(file);
            RequestCache cache = JsonConvert.DeserializeObject<RequestCache>(json);
            txAction.Text = cache.Action;
            txController.Text = cache.Controller;

            if (cache != null)
                if (cache.Parameters != null)
                    cache.Parameters.Add(new ServerRequestParameter());

            if (cache.Parameters.Count > 0)
                dataGrid.DataSource = cache.Parameters;

            return true;
        }

        private void SaveCache()
        {
            string dir = @".\RequestCaches\";
            string file = $@"{dir}\{txController.Text}.{txAction.Text}.json";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            RequestCache cache = new RequestCache();
            cache.Action = txAction.Text;
            cache.Controller = txController.Text;
            cache.Parameters = ((IEnumerable)dataGrid.DataSource)
                .Cast<ServerRequestParameter>()
                .Where(p => !string.IsNullOrEmpty(p.Key))
                .ToList();

            string json = JsonConvert.SerializeObject(cache);
            File.WriteAllText(file, json);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button1.Text = "Sending request...";

            var list = ((IEnumerable)dataGrid.DataSource)
                .Cast<ServerRequestParameter>()
                .Where(p => !string.IsNullOrEmpty(p.Key))
                .ToList();
            string controller = txController.Text;
            string action = txAction.Text;

            if (ckFileResult.Checked)
            {
                DownloadFile(controller, action, list);
                Result = new OperationResult()
                {
                    Status = 600,
                    Entity = "",
                    Message = "File saved"
                };
                Close();
                return;
            }

            SaveCache();

            OperationResult res = await Task.Run(() =>
            {
                try
                {
                    Client client = new Client();
                    RequestBody rb = RequestBody.Create(controller, action);
                    foreach (var parameter in list)
                        rb.AddParameter(parameter.Key, parameter.Value);

                    client.SendRequest(rb);

                    var result = client.GetResult();
                    ServerResponse = client.Response;
                    return result;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: \n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            });

            Result = res;
            Close();
        }

        private void DownloadFile(string controller, string action, List<ServerRequestParameter> list)
        {
            byte[] res;

            try
            {
                Client client = new Client();
                RequestBody rb = RequestBody.Create(controller, action);
                foreach (var parameter in list)
                    rb.AddParameter(parameter.Key, parameter.Value);

                client.SendRequest(rb);
                res = client.GetResultFile();
                ServerResponse = client.Response;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: \n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.ShowDialog();

            if (string.IsNullOrEmpty(dialog.FileName))
                return;


            File.WriteAllBytes(dialog.FileName, res);
        }
    }
}
