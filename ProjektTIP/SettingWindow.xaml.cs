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

namespace ProjektTIP
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        private Friend user;

        public SettingWindow(ref Friend user)
        {
            InitializeComponent();
            this.user = user;
            this.DataContext = this.user;
        }

        private void bSave_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
