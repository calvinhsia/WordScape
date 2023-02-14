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

namespace WordScape
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : UserControl
    {
        public bool SaveWasClicked;
        public Options(WordScapeOptions WordScapeOptions)
        {
            InitializeComponent();
            this.DataContext = WordScapeOptions;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveWasClicked = true;
            Window.GetWindow(this).Close();
        }
    }
}
