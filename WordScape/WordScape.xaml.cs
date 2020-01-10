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
using System.Windows.Threading;

namespace WordScape
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WordScapeWindow : Window, INotifyPropertyChanged
    {
        internal WordGenerator _wordGen;
        internal WordContainer _WordCont;
        internal GenGrid _gridgen;
        static internal WordScapeWindow WordScapeWindowInstance;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnMyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        string _strWordSoFar;
        public string StrWordSoFar { get { return _strWordSoFar; } set { _strWordSoFar = value; OnMyPropertyChanged(); } }

        public int LenTargetWord { get; set; } = 7;
        public int MinSubWordLength { get; set; } = 5;
        private readonly Random _random;

        private int _CountDownTime;
        public bool TimerIsEnabled = false;
        public int CountDownTime { get { return _CountDownTime; } set { _CountDownTime = value; OnMyPropertyChanged("CountDownTimeStr"); } }
        public string CountDownTimeStr
        {
            get
            {
                return GetTimeAsString(_CountDownTime);
            }
        }

        private string GetTimeAsString(int tmpSecs)
        {
            var hrs = string.Empty;
            var mins = string.Empty;
            if (tmpSecs >= 3600)
            {
                hrs = $"{(int)(tmpSecs / 3600):n0}:";
                tmpSecs -= (int)((tmpSecs / 3600)) * 3600;
            }
            string secs;
            if (!string.IsNullOrEmpty(hrs) || tmpSecs >= 60)
            {
                mins = $"{((int)((tmpSecs / 60))).ToString(String.IsNullOrEmpty(hrs) ? "" : "00")}:";
                tmpSecs -= (int)((tmpSecs / 60)) * 60;
                secs = tmpSecs.ToString("00");
            }
            else
            {
                secs = tmpSecs.ToString();
            }
            return $"{hrs}{mins}{secs}";
        }

        private ObservableCollection<UIElement> _LstWrdsSoFar = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> LstWrdsSoFar { get { return _LstWrdsSoFar; } set { _LstWrdsSoFar = value; OnMyPropertyChanged(); } }

        private int _NumHintsUsed;
        public int NumHintsUsed { get { return _NumHintsUsed; } internal set { _NumHintsUsed = value; OnMyPropertyChanged(); } }

        private int _NumWordsFound;
        public int NumWordsFound { get { return _NumWordsFound; } internal set { _NumWordsFound = value; OnMyPropertyChanged(); } }

        public int NumWordsTotal { get { return _gridgen == null ? 0 : _gridgen.NumWordsPlaced; } internal set { OnMyPropertyChanged(); } }


        public WordScapeWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Width = Properties.Settings.Default.WindowSize.Width;
            this.Height = Properties.Settings.Default.WindowSize.Height;
            this.Top = Properties.Settings.Default.WindowPos.Y;
            this.Left = Properties.Settings.Default.WindowPos.X;
            WordScapeWindowInstance = this;
            _random = new Random(
#if DEBUG
                    //                        1
#endif
                    );
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
            var timer = new DispatcherTimer(
                TimeSpan.FromSeconds(1),
                DispatcherPriority.Normal,
                (o, et) =>
                {
                    if (TimerIsEnabled)
                    {
                        CountDownTime += 1;
                    }
                },
                this.Dispatcher
                );
            BtnPlayAgain.RaiseEvent(new RoutedEventArgs() { RoutedEvent = Button.ClickEvent, Source = this });
        }

        void BtnShuffle_Click(object sender, RoutedEventArgs e)
        {
            this.ltrWheel.Shuffle();
        }

        private void BtnPlayAgain_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.LenTargetWord >= 12)
                {
                    var ow = new Window()
                    {
                        Left = this.Left,
                        Top = this.Top,
                        Height = 300,
                        Width = 800,
                    };
                    ow.Content = "Really ??";
                    ow.ShowDialog();
                    return;
                }
                this._wordGen = new WordGenerator(_random,
                    minSubWordLen: MinSubWordLength,
                    numMaxSubWords: 1500);

                _WordCont = this._wordGen.GenerateWord(LenTargetWord);
                _gridgen = new GenGrid(maxX: 12, maxY: 12, _WordCont, this._wordGen._rand);
                FillGrid(_gridgen);
                NumWordsTotal = 0; // force prop changed
                NumWordsFound = 0;
                StrWordSoFar = string.Empty;
                LstWrdsSoFar.Clear();
                CountDownTime = 0;
                //this.ltrWheel = new LetterWheel();
                //Grid.SetRow(this.ltrWheel, 3);
                this.ltrWheel.LetterWheelInit(this, _WordCont, _gridgen);
                TimerIsEnabled = true;
            }
            catch (Exception ex)
            {
                this.Content = ex.ToString();
            }
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
                    var ltrTile = new LtrTile(gridgen._chars[x, y]);
                    unigrid.Children.Add(ltrTile);
                }
            }
        }
    }

    public class LtrTile : DockPanel
    {
        private readonly char _ltr;
        public bool IsShowing;
        internal readonly TextBlock txtBlock;
        public LtrTile(char ltr)
        {
            this._ltr = ltr;
            Margin = new Thickness(2, 2, 2, 2);
            if (ltr != GenGrid.Blank)
            {
                Background = Brushes.DarkCyan;
                txtBlock = new TextBlock()
                {
                    Text = ltr == GenGrid.Blank ? " " : ltr.ToString().ToUpper(),
                    FontSize = 20,
                    Foreground = Brushes.White,
                    //                    Background = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Visibility = Visibility.Hidden
                };
                this.Children.Add(txtBlock);
            }
            else
            {
                this.txtBlock = new TextBlock();
                this.Children.Add(txtBlock);
                IsShowing = true;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (!IsShowing)
            {
                WordScapeWindow.WordScapeWindowInstance.NumHintsUsed++;
                var anim = new ObjectAnimationUsingKeyFrames
                {
                    Duration = TimeSpan.FromMilliseconds(2500), // Dura of entire timeline
                                                                //                    RepeatBehavior = new RepeatBehavior(10) // # times to repeat duration. Total dura = RepeatCount * Dura
                };
                var frm1 = new DiscreteObjectKeyFrame(Visibility.Visible, TimeSpan.FromMilliseconds(10));
                anim.KeyFrames.Add(frm1);
                var frm2 = new DiscreteObjectKeyFrame(Visibility.Hidden, TimeSpan.FromMilliseconds(450));
                anim.KeyFrames.Add(frm2);
                anim.FillBehavior = FillBehavior.Stop;
                this.txtBlock.BeginAnimation(TextBlock.VisibilityProperty, anim);


                //var storyBoard = this.wordScapeWindow.TryFindResource("blinkAnimation") as Storyboard;
                //var name = $"LtrTile{x}{y}";
                //this.RegisterName(name, this.wordScapeWindow);
                //var xx1 = storyBoard.Children[0] as ColorAnimationUsingKeyFrames;
                //xx1.FillBehavior = FillBehavior.Stop;
                //xx1.RepeatBehavior= new RepeatBehavior(40);
                //xx1.Duration = new Duration(TimeSpan.FromMilliseconds(1011));
                //Storyboard.SetTarget(xx1, this.txtBlock);
                //Storyboard.SetTargetName(xx1, name);

                //var xx2 = storyBoard.Children[1] as ColorAnimationUsingKeyFrames;
                //Storyboard.SetTarget(xx2, this.txtBlock);
                //Storyboard.SetTargetName(xx2, name);
                //xx2.FillBehavior = FillBehavior.Stop;
                //xx2.RepeatBehavior = new RepeatBehavior(40);
                //xx2.Duration = new Duration(TimeSpan.FromMilliseconds(1011));

                //storyBoard?.Begin();
                ////https://stackoverflow.com/questions/4177574/how-to-make-a-textblock-blink-in-wpf

            }
        }
        internal void ShowLetter()
        {
            if (!IsShowing && this._ltr != GenGrid.Blank)
            {
                this.txtBlock.Visibility = Visibility.Visible;
                IsShowing = true;
            }
        }
        public override string ToString()
        {
            return $"{_ltr}";
        }
    }

}
