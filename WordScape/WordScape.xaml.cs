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
using System.Windows.Media.Animation;
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
            FillGrid(gridgen);
            StrWordSoFar = string.Empty;
            LstWrdsSoFar.Clear();
            //this.ltrWheel = new LetterWheel();
            //Grid.SetRow(this.ltrWheel, 3);
            this.ltrWheel.LetterWheelInit(this, WordCont, gridgen);
        }

        private void FillGrid(GenGrid gridgen)
        {
            unigrid.Children.Clear();
            unigrid.Columns = gridgen._MaxX;
            unigrid.Rows = gridgen._MaxY;
            //            unigrid.Background = Brushes.Black;
            for (int y = 0; y < gridgen._MaxY; y++)
            {
                for (int x = 0; x < gridgen._MaxX; x++)
                {
                    //unigrid.Children.Add(new TextBlock() { Text = "AA" });
                    var ltrTile = new LtrTile(this, gridgen._chars[x, y], x, y);
                    unigrid.Children.Add(ltrTile);
                }
            }
        }

        private void BtnShowLtrs_Click(object sender, RoutedEventArgs e)
        {
            foreach (LtrTile tile in this.unigrid.Children)
            {
                tile.ShowLetter();
            }
        }
    }

    public class LtrTile : DockPanel
    {
        private readonly char v;
        private readonly int x;
        private readonly int y;
        public bool IsShowing;
        private readonly WordScapeWindow wordScapeWindow;
        private readonly TextBlock txtBlock;
        public LtrTile(WordScapeWindow wordScapeWindow, char v, int x, int y)
        {
            this.wordScapeWindow = wordScapeWindow;
            this.v = v;
            this.x = x;
            this.y = y;
            Margin = new Thickness(2, 2, 2, 2);
            if (v != GenGrid.Blank)
            {
                Background = Brushes.DarkCyan;
                txtBlock = new TextBlock()
                {
                    //                        Text = v == Blank ? " " : v.ToString().ToUpper(),
                    FontSize = 20,
                    Foreground = Brushes.White,
                    Background = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                    , Text="A"
                };
                this.Children.Add(txtBlock);
            }
            else
            {
                this.Children.Add(new TextBlock());
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (IsShowing)
            {
                IsShowing = false;
                (this.Children[0] as TextBlock).Text = " ";
            }
            else
            {
                var storyBoard = this.wordScapeWindow.TryFindResource("blinkAnimation") as Storyboard;
                var name = $"LtrTile{x}{y}";
                this.RegisterName(name, this.wordScapeWindow);
                var xx1 = storyBoard.Children[0] as ColorAnimationUsingKeyFrames;
                xx1.FillBehavior = FillBehavior.Stop;
                xx1.RepeatBehavior= new RepeatBehavior(40);
                xx1.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
                Storyboard.SetTarget(xx1, this.txtBlock);
                Storyboard.SetTargetName(xx1, name);

                var xx2 = storyBoard.Children[1] as ColorAnimationUsingKeyFrames;
                Storyboard.SetTarget(xx2, this.txtBlock);
                Storyboard.SetTargetName(xx2, name);
                xx2.FillBehavior = FillBehavior.Stop;
                xx2.RepeatBehavior = new RepeatBehavior(40);
                xx2.Duration = new Duration(TimeSpan.FromMilliseconds(1000));

                storyBoard?.Begin();
                //https://stackoverflow.com/questions/4177574/how-to-make-a-textblock-blink-in-wpf

            }
        }
        internal void ShowLetter()
        {
            if (this.v != GenGrid.Blank)
            {
                (this.Children[0] as TextBlock).Text = v.ToString().ToUpper();
                IsShowing = true;
            }
        }
    }

}
