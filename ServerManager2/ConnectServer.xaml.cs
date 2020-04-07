using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace ServerManager2
{
    /// <summary>
    /// Lógica interna para ConnectServer.xaml
    /// </summary>
    public partial class ConnectServer : Window
    {
        public ConnectServer()
        {
            InitializeComponent();
        }

        private void CkAuth_Checked(object sender, RoutedEventArgs e)
        {
            gridAuth.Visibility = Visibility.Visible;
            Height = 416.545d;
        }

        private void CkAuth_Unchecked(object sender, RoutedEventArgs e)
        {
            gridAuth.Visibility = Visibility.Hidden;
            Height = 303.211;
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btConnect_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
