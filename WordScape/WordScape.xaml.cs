using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WordGenerator wordGen;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.wordGen = new WordGenerator(new Random(1));

                BtnPlayAgain.RaiseEvent(new RoutedEventArgs() { RoutedEvent = Button.ClickEvent, Source = this });
            }
            catch (Exception ex)
            {
                this.Content = ex.ToString();
            }
        }

        private void BtnPlayAgain_Click(object sender, RoutedEventArgs e)
        {
            var WordCont = this.wordGen.GenerateWord(Targetlen: 7, numMaxSubWords: 15);
            var gridgen = new GenGrid(maxX: 10, maxY: 10, WordCont, this.wordGen._rand);
            gridgen.CreateUserControl(this.unigrid);
        }
    }
}
