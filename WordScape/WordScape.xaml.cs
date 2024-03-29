﻿using System;
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
    public class WordScapeOptions : WordGenerationParms
    {
        public bool IsWordScape { get; set; } = true; // or Ruffle
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WordScapeWindow : Window, INotifyPropertyChanged
    {
        WordScapePuzzle _wordScapePuzzleCurrent = new();
        Task<WordScapePuzzle> taskGenNextPuzzle;

        readonly WordScapeOptions _WordScapeOptions = new();

        internal WordGenerator _wordGen { get { return _wordScapePuzzleCurrent.wordGenerator; } set { _wordScapePuzzleCurrent.wordGenerator = value; } }
        internal WordContainer _WordCont { get { return _wordScapePuzzleCurrent.wordContainer; } set { _wordScapePuzzleCurrent.wordContainer = value; } }
        internal GenGrid _gridGen { get { return _wordScapePuzzleCurrent.genGrid; } set { _wordScapePuzzleCurrent.genGrid = value; } }
        static internal WordScapeWindow WordScapeWindowInstance;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnMyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        string _strWordSoFar;
        public string StrWordSoFar { get { return _strWordSoFar; } set { _strWordSoFar = value; OnMyPropertyChanged(); } }

        public int LenTargetWord { get { return _WordScapeOptions.LenTargetWord; } set { _WordScapeOptions.LenTargetWord = value; } }
        public int MinSubWordLength { get { return _WordScapeOptions.MinSubWordLength; } set { _WordScapeOptions.MinSubWordLength = value; } }

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

        private ObservableCollection<UIElement> _LstWrdsSoFar = new();
        public ObservableCollection<UIElement> LstWrdsSoFar { get { return _LstWrdsSoFar; } set { _LstWrdsSoFar = value; OnMyPropertyChanged(); } }

        private int _NumHintsUsed;
        public int NumHintsUsed { get { return _NumHintsUsed; } internal set { _NumHintsUsed = value; OnMyPropertyChanged(); } }
        public int NumItemsInList { get { return _LstWrdsSoFar.Count; } }

        private int _NumWordsFound;
        private DispatcherTimer _timer;

        public int NumWordsFound { get { return _NumWordsFound; } internal set { _NumWordsFound = value; OnMyPropertyChanged(); } }

        public int NumWordsTotal { get { return _gridGen == null ? 0 : _gridGen.NumWordsPlaced; } internal set { OnMyPropertyChanged(); } }


        public WordScapeWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Width = Properties.Settings.Default.WindowSize.Width;
            this.Height = Properties.Settings.Default.WindowSize.Height;
            this.Top = Properties.Settings.Default.WindowPos.Y;
            this.Left = Properties.Settings.Default.WindowPos.X;
            this.LenTargetWord = Properties.Settings.Default.WordLen;
            this.MinSubWordLength = Properties.Settings.Default.SubWordLen;
            this._WordScapeOptions.MaxX = Properties.Settings.Default.MaxX;
            this._WordScapeOptions.MaxY = Properties.Settings.Default.MaxY;
            WordScapeWindowInstance = this;
            _WordScapeOptions._Random = new Random(
#if DEBUG
                    //1
#endif
                    );
            this.Closing += (o, ec) =>
            {
                _timer?.Stop();
                Properties.Settings.Default.WindowPos = new System.Drawing.Point((int)this.Left, (int)this.Top);
                Properties.Settings.Default.WindowSize = new System.Drawing.Size((int)this.Width, (int)this.Height);
                Properties.Settings.Default.WordLen = this.LenTargetWord;
                Properties.Settings.Default.SubWordLen = this.MinSubWordLength;
                Properties.Settings.Default.MaxX = _WordScapeOptions.MaxX;
                Properties.Settings.Default.MaxY = _WordScapeOptions.MaxY;
                Properties.Settings.Default.Save();
            };
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _timer = new DispatcherTimer(
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
            taskGenNextPuzzle = WordScapePuzzle.CreateNextPuzzleTask(_WordScapeOptions); // don't await it here
            BtnNew.RaiseEvent(new RoutedEventArgs() { RoutedEvent = Button.ClickEvent, Source = this });
        }

        void BtnShuffle_Click(object sender, RoutedEventArgs e)
        {
            this.ltrWheel.Shuffle();
        }

        private async void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.LenTargetWord >= 12)
                {
                    var ow = new Window
                    {
                        Left = this.Left,
                        Top = this.Top,
                        Height = 300,
                        Width = 800,
                        Content = "Really ??"
                    };
                    ow.ShowDialog();
                    return;
                }
                this.BtnNew.IsEnabled = false;
                this.wrdsSoFar.RenderTransform = Transform.Identity;
                TimerIsEnabled = false;
                var done = false;
                WordScapePuzzle newpuzzle = null;
                while (!done)
                {
                    try
                    {
                        newpuzzle = await taskGenNextPuzzle;
                        if (newpuzzle.LenTargetWord == _WordScapeOptions.LenTargetWord && newpuzzle.MinSubWordLength == _WordScapeOptions.MinSubWordLength)
                        {
                            done = true;
                        }
                        taskGenNextPuzzle = WordScapePuzzle.CreateNextPuzzleTask(_WordScapeOptions); // set up for next puzzle to be instantaneous
                    }
                    catch (Exception ex)
                    {
                        // old dict threw sometimes
                        StrWordSoFar = ex.ToString();
                        done = true;
                    }
                }
                _wordScapePuzzleCurrent = newpuzzle;

                FillGrid(_gridGen);
                NumWordsTotal = 0; // force prop changed
                NumWordsFound = 0;
                StrWordSoFar = string.Empty;
                LstWrdsSoFar.Clear();
                CountDownTime = 0;
                //this.ltrWheel = new LetterWheel();
                //Grid.SetRow(this.ltrWheel, 3);
                this.ltrWheel.LetterWheelInit(this, _WordCont, _gridGen);
                TimerIsEnabled = true;
            }
            catch (Exception ex)
            {
                this.Content = ex.ToString();
            }
            this.BtnNew.IsEnabled = true;
        }
        async void BtnOptions_Click(object sender, RoutedEventArgs e)
        {
            var optionsPage = new Options(_WordScapeOptions);
            var w = new Window()
            {
                Content = optionsPage,
                Owner = this
            };
            w.ShowDialog();
            if (optionsPage.SaveWasClicked)
            {
                try
                {
                    await taskGenNextPuzzle; // ensure old one finished
                    taskGenNextPuzzle = WordScapePuzzle.CreateNextPuzzleTask(_WordScapeOptions);
                    BtnNew.RaiseEvent(new RoutedEventArgs() { RoutedEvent = Button.ClickEvent, Source = this });
                }
                catch (Exception ex)
                {
                    this.StrWordSoFar = ex.ToString();
                }
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
                    var ltrTile = new LtrTile(gridgen, x, y);
                    unigrid.Children.Add(ltrTile);
                }
            }
        }

        private void WrapPanel_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            //            e.IsSingleTouchEnabled = false;
            e.ManipulationContainer = this;
            e.Handled = true;
        }

        private void WrapPanel_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            //this just gets the source. 
            // I cast it to FE because I wanted to use ActualWidth for Center. You could try RenderSize as alternate
            if (e.Source is FrameworkElement element)
            {
                //e.DeltaManipulation has the changes 
                // Scale is a delta multiplier; 1.0 is last size,  (so 1.1 == scale 10%, 0.8 = shrink 20%) 
                // Rotate = Rotation, in degrees
                // Pan = Translation, == Translate offset, in Device Independent Pixels 

                var deltaManipulation = e.DeltaManipulation;
                var matrix = ((MatrixTransform)element.RenderTransform).Matrix;
                // find the old center; arguaby this could be cached 
                Point center = new(element.ActualWidth / 2, element.ActualHeight / 2);
                // transform it to take into account transforms from previous manipulations 
                center = matrix.Transform(center);
                //this will be a Zoom. 
                matrix.ScaleAt(deltaManipulation.Scale.X, deltaManipulation.Scale.Y, center.X, center.Y);
                // Rotation 
                matrix.RotateAt(e.DeltaManipulation.Rotation, center.X, center.Y);
                //Translation (pan) 
                matrix.Translate(e.DeltaManipulation.Translation.X, e.DeltaManipulation.Translation.Y);

                element.RenderTransform = new MatrixTransform(matrix);
                //                ((MatrixTransform)element.RenderTransform).Matrix = matrix;

                e.Handled = true;
            }
        }

        private void WrapPanel_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            // Decrease the velocity of the Rectangle's movement by 
            // 10 inches per second every second.
            // (10 inches * 96 pixels per inch / 1000ms^2)
            e.TranslationBehavior.DesiredDeceleration = 10.0 * 96.0 / (1000.0 * 1000.0);

            // Decrease the velocity of the Rectangle's resizing by 
            // 0.1 inches per second every second.
            // (0.1 inches * 96 pixels per inch / (1000ms^2)
            e.ExpansionBehavior.DesiredDeceleration = 0.1 * 96 / (1000.0 * 1000.0);

            // Decrease the velocity of the Rectangle's rotation rate by 
            // 2 rotations per second every second.
            // (2 * 360 degrees / (1000ms^2)
            e.RotationBehavior.DesiredDeceleration = 720 / (1000.0 * 1000.0);

            e.Handled = true;
        }
    }

    public class LtrTile : DockPanel
    {
        private readonly char _ltr;
        public bool IsShowing { get; internal set; }
        internal readonly TextBlock txtBlock;
        public LtrTile(GenGrid gengrid, int x, int y)
        {
            var ltr = gengrid._chars[x, y];
            this._ltr = ltr;
            var fontsize = 20;
            if (gengrid._MaxX > 12)
            {
                fontsize = 17;
            }
            Margin = new Thickness(1, 1, 1, 1);
            if (ltr != GenGrid.Blank)
            {
                Background = Brushes.DarkCyan;
                txtBlock = new TextBlock()
                {
                    Text = ltr == GenGrid.Blank ? " " : ltr.ToString().ToUpper(),
                    FontSize = fontsize,
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
