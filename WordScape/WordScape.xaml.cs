using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class WordScapeWindow : Window, INotifyPropertyChanged
    {
        internal WordGenerator _wordGen;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnMyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        string _strWordSoFar;
        public string StrWordSoFar { get { return _strWordSoFar; } set { _strWordSoFar = value; OnMyPropertyChanged(); } }

        private ObservableCollection<UIElement> _LstWrdsSoFar = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> LstWrdsSoFar { get { return _LstWrdsSoFar; } set { _LstWrdsSoFar = value; OnMyPropertyChanged(); } }
        public WordScapeWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Width = Properties.Settings.Default.WindowSize.Width;
            this.Height = Properties.Settings.Default.WindowSize.Height;
            this.Top = Properties.Settings.Default.WindowPos.Y;
            this.Left = Properties.Settings.Default.WindowPos.X;
            this.Closing += (o, ec) =>
            {
                Properties.Settings.Default.WindowPos = new System.Drawing.Point((int)this.Left, (int)this.Top);
                Properties.Settings.Default.WindowSize = new System.Drawing.Size((int)this.Width, (int)this.Height);
                Properties.Settings.Default.Save();
            };
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var rand = new Random(
#if DEBUG
                        1
#endif
                    );
                this._wordGen = new WordGenerator(rand,
                    minSubWordLen: 5,
                    numMaxSubWords: 1500);

                BtnPlayAgain.RaiseEvent(new RoutedEventArgs() { RoutedEvent = Button.ClickEvent, Source = this });
            }
            catch (Exception ex)
            {
                this.Content = ex.ToString();
            }
        }

        private void BtnPlayAgain_Click(object sender, RoutedEventArgs e)
        {
            var WordCont = this._wordGen.GenerateWord(Targetlen: 7);
            var gridgen = new GenGrid(maxX: 12, maxY: 12, WordCont, this._wordGen._rand);
            gridgen.FillGrid(this.unigrid);
            StrWordSoFar = string.Empty;
            LstWrdsSoFar.Clear();
            //this.ltrWheel = new LetterWheel();
            //Grid.SetRow(this.ltrWheel, 3);
            this.ltrWheel.LetterWheelInit(this, WordCont, gridgen);
        }
        private void BtnShowLtrs_Click(object sender, RoutedEventArgs e)
        {
            foreach (LtrTile tile in this.unigrid.Children)
            {
                tile.ShowLetter();
            }
        }
    }
}
