using SocketAppServerClient;
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
    }
}
