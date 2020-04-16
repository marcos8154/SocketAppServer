using MobileAppServerClient;
using ServerManager2.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ServerManager2
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            listConnections.Items.Add(new ConnectionUC());
            listConnections.Items.Add(new ConnectionUC());
            listConnections.Items.Add(new ConnectionUC());
            listConnections.Items.Add(new ConnectionUC());
        }

        private void BtConnect_Click(object sender, RoutedEventArgs e)
        {
            ConnectServer cs = new ConnectServer();
            cs.ShowDialog();
        }

        private void listConnections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (ConnectionUC item in listConnections.Items)
                item.SetUnselected();

            var selected = (ConnectionUC)listConnections.SelectedItem;
            selected.SetSelected();
        }

        private Address GetAddress()
        {
            return new Address
            {
                Street = "Rua XPTO",
                City = "Cidade dos Navegantes"
            };
        }

        private Entity GetEntity()
        {
            return new Entity
            {
                Name = "Entity Name",
                Phone = "24 998741181"
            };
        }

        private List<decimal> newMethod = new List<decimal>();
        private List<decimal> legacyMethod = new List<decimal>();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Client client = null;
            try
            {
                List<Entity> entities = new List<Entity>();
                for (int i = 0; i < 50; i++)
                    entities.Add(new Entity { Name = $"Entity {i + 1}", Phone = $"{new Random(i + 1).Next()}" });


                Client.Configure("localhost", 4000, 1024 * 100);
                client = new Client();
                RequestBody rb = RequestBody.Create("TestController", "SaveEntity")
                    .AddParameter("entity", GetEntity())
                    .AddParameter("address", GetAddress())
                    .AddParameter("entities", entities)
                    .AddParameter("active", true)
                    .AddParameter("alias", "Test")
                    .AddParameter("anotherParameter", "parameter X");

                Stopwatch sw = new Stopwatch();
                sw.Start();
                client.SendRequest(rb);
                client.GetResult();
                sw.Stop();

                newMethod.Add(sw.ElapsedMilliseconds);
                Recalc();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Recalc()
        {
            decimal avgLegacy = (legacyMethod.Count == 0
                ? 0
                : legacyMethod.Average());
            decimal avgNew = (newMethod.Count == 0
                ? 0
                : newMethod.Average());

            decimal lastLegacy = (legacyMethod.Count == 0
                ? 0
                : legacyMethod.LastOrDefault());
            decimal lastNew = (newMethod.Count == 0
                ? 0
                : newMethod.LastOrDefault());

            lbLegacy.Content = $"AVG ~{avgLegacy.ToString("N2")}ms; LAST {lastLegacy.ToString("N2")}ms";
            lbNew.Content = $"AVG ~{avgNew.ToString("N2")}ms; LAST {lastNew.ToString("N2")}ms";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Client client = null;
            try
            {
                var entity = GetEntity();
                var address = GetAddress();

                Client.Configure("localhost", 4000, 1024 * 100);
                client = new Client();
                RequestBody rb = RequestBody.Create("TestController", "SaveEntity")
                    .AddParameter("entity.Name", entity.Name)
                    .AddParameter("entity.Phone", entity.Phone)
                    .AddParameter("address.Street", address.Street)
                    .AddParameter("address.City", address.City)
                    .AddParameter("active", true)
                    .AddParameter("alias", "Test")
                    .AddParameter("anotherParameter", "parameter X");

                Stopwatch sw = new Stopwatch();
                sw.Start();
                client.SendRequest(rb);
                client.GetResult();
                sw.Stop();

                legacyMethod.Add(sw.ElapsedMilliseconds);
                Recalc();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                // if (client != null)
                //      client.Close();
            }
        }
    }
}
