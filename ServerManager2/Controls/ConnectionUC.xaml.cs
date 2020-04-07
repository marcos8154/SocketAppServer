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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ServerManager2.Controls
{
    /// <summary>
    /// Interação lógica para ConnectionUC.xam
    /// </summary>
    public partial class ConnectionUC : UserControl
    {
        public ConnectionUC()
        {
            InitializeComponent();
        }

        public void SetSelected()
        {
            lbAlias.Foreground = (Brush)new BrushConverter().ConvertFromString("White");
        }

        public void SetUnselected()
        {
            lbAlias.Foreground = (Brush)new BrushConverter().ConvertFromString("#FF6A6A6A");
        }
    }
}
