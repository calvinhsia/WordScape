using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WordScape
{
    public class LetterWheel : Canvas
    {
        enum FoundWordType
        {
            FoundWordTypeSubWordInGrid,
            FoundWordTypeSubWordNotInGrid
        };
        class FoundWord
        {
            public FoundWordType foundStringType;
            public string word;
        }
        private WordScapeWindow mainWindow;
        private WordContainer wordCont;
        private GenGrid gridgen;
        int _WordsFound;

        private List<LetterWheelLetter> _lstLetters = new List<LetterWheelLetter>();
        private Polyline polyLine = new Polyline()
        {
            Stroke = Brushes.Red,
            StrokeThickness = 10
        };
        private bool _mouseIsDown = false;
        private bool _curlinefloating = false;
        private List<LetterWheelLetter> _lstLtrsSelected = new List<LetterWheelLetter>();
        private List<FoundWord> _lstFoundWordsSoFar = new List<FoundWord>();
        public LetterWheel()
        {

        }
        public void LetterWheelInit(WordScapeWindow mainWindow, WordContainer wordCont, GenGrid gridgen)
        {
            this.mainWindow = mainWindow;
            this.wordCont = wordCont;
            this.gridgen = gridgen;
            this.CreateCircle();
            _WordsFound = 0;
            _lstFoundWordsSoFar.Clear();
        }

        private void CreateCircle()
        {
            this.Children.Clear();
            this.Background = Brushes.AliceBlue;
            var circRadius = 110;
            var circ = new Ellipse()
            {
                Width = 2 * circRadius,
                Height = 2 * circRadius,
                Fill = Brushes.White,
                StrokeThickness = 3,
                Stroke = Brushes.Black
            };
            var ptcircpos = new Point(60, 10);
            Canvas.SetLeft(circ, ptcircpos.X);
            Canvas.SetTop(circ, ptcircpos.Y);
            var ptCircCtr = new Point(ptcircpos.X + circ.Width / 2, ptcircpos.Y + circ.Height / 2);
            this.Children.Add(circ);

            var numLtrs = wordCont.InitialWord.Length;
            int ndx = 0;
            var radsPerLetter = (2 * Math.PI / numLtrs);
            foreach (var ltr in wordCont.InitialWord.ToUpper().OrderBy(p => gridgen._random.NextDouble()))
            //                foreach (var ltr in wordCont.InitialWord.ToUpper().OrderBy(p => Guid.NewGuid()))
            {
                var lett = new LetterWheelLetter(ltr);
                _lstLetters.Add(lett);

                var x = ptCircCtr.X + .7 * circRadius * Math.Cos(radsPerLetter * ndx) - lett.Width / 2;
                var y = ptCircCtr.Y - .7 * circRadius * Math.Sin(radsPerLetter * ndx) - lett.Height / 2;
                var letpt = new Point(x, y);
                Canvas.SetLeft(lett, letpt.X);
                Canvas.SetTop(lett, letpt.Y);
                this.Children.Add(lett);
                if (ndx % 2 == 0)
                {
                    //                    lett.Select();
                }
                ndx++;
            }

            this.Children.Add(polyLine);
        }
        LetterWheelLetter ltrFromArgs(MouseEventArgs args)
        {
            var pt = args.GetPosition(this);
            LetterWheelLetter ltrUnderMouse = null;
            var x = this.InputHitTest(pt);
            if (x != null)
            {
                if (x is TextBlock)
                {
                    ltrUnderMouse = VisualTreeHelper.GetParent(x as TextBlock) as LetterWheelLetter;
                }
                else if (x is LetterWheelLetter)
                {
                    ltrUnderMouse = x as LetterWheelLetter;
                }
            }
            return ltrUnderMouse;
        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            polyLine.Points.Clear();
            _lstLtrsSelected.Clear();
            var ltrUnderMouse = ltrFromArgs(e);
            if (ltrUnderMouse != null)
            {
                ltrUnderMouse.Select();
                _lstLtrsSelected.Add(ltrUnderMouse);
                this.mainWindow.StrWordSoFar = ltrUnderMouse.ltr.ToString();
                _mouseIsDown = true;
                var pt = ltrUnderMouse.TranslatePoint(new Point(0, 0), this);
                pt.X += ltrUnderMouse.Width / 2;
                pt.Y += ltrUnderMouse.Height / 2;
                polyLine.Points.Add(pt);
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_curlinefloating)
            {
                if (polyLine.Points.Count > 0)
                {
                    polyLine.Points.RemoveAt(polyLine.Points.Count - 1);
                }
                _curlinefloating = false;
            }
            if (_mouseIsDown)
            {
                var ltrUnderMouse = ltrFromArgs(e);
                if (ltrUnderMouse != null)
                {
                    if (_lstLtrsSelected.Contains(ltrUnderMouse))
                    {
                        if (_lstLtrsSelected.Count > 1)
                        {
                            var at = _lstLtrsSelected.IndexOf(ltrUnderMouse);
                            if (at == _lstLtrsSelected.Count - 2)
                            {
                                _lstLtrsSelected[_lstLtrsSelected.Count - 1].UnSelect();
                                _lstLtrsSelected.RemoveAt(_lstLtrsSelected.Count - 1);
                                polyLine.Points.RemoveAt(polyLine.Points.Count - 1);
                                polyLine.Points.Add(e.GetPosition(this));
                                _curlinefloating = true;
                            }
                        }
                    }
                    else
                    {
                        ltrUnderMouse.Select();
                        _lstLtrsSelected.Add(ltrUnderMouse);
                        this.mainWindow.StrWordSoFar += ltrUnderMouse.ltr.ToString();
                        var pt = ltrUnderMouse.TranslatePoint(new Point(0, 0), this);
                        pt.X += ltrUnderMouse.Width / 2;
                        pt.Y += ltrUnderMouse.Height / 2;
                        polyLine.Points.Add(pt);
                        _curlinefloating = false;
                    }
                }
                else
                {
                    polyLine.Points.Add(e.GetPosition(this));
                    _curlinefloating = true;
                }
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            var wrdSoFar = this.mainWindow.StrWordSoFar;
            if (wrdSoFar.Length >= this.mainWindow._wordGen._MinSubWordLen)
            {
                if (this.wordCont.subwords.Contains(wrdSoFar)) // if it's one of the subwords
                {
                    var doRefreshList = false;
                    if (_lstFoundWordsSoFar.Where(p => p.word == wrdSoFar).Any()) // user already found this word
                    {

                    }
                    else
                    {
                        var stat = this.gridgen.ShowWord(wrdSoFar);
                        switch (stat)
                        {
                            case GenGrid.WordStatus.IsNotInGrid:
                                _lstFoundWordsSoFar.Add(new FoundWord() { foundStringType = FoundWordType.FoundWordTypeSubWordNotInGrid, word = wrdSoFar });
                                doRefreshList = true;
                                break;
                            case GenGrid.WordStatus.IsAlreadyInGrid:
                                break;
                            case GenGrid.WordStatus.IsShownInGridForFirstTime:
                                _lstFoundWordsSoFar.Add(new FoundWord() { foundStringType = FoundWordType.FoundWordTypeSubWordInGrid, word = wrdSoFar });
                                _WordsFound++;
                                doRefreshList = true;
                                break;
                        }
                    }
                    if (doRefreshList)
                    {
                        this.RefreshWordList();
                    }
                }
            }
            _mouseIsDown = false;
            polyLine.Points.Clear();
            _curlinefloating = false;
            foreach (var ltr in _lstLetters)
            {
                ltr.UnSelect();
            }
            _lstLtrsSelected.Clear();
            if (_WordsFound == this.gridgen._dictPlacedWords.Count)
            {
                this.mainWindow.StrWordSoFar = "YAYYY!";
            }
            else
            {
                this.mainWindow.StrWordSoFar = string.Empty;
            }
        }
        void RefreshWordList()
        {
            this.mainWindow.LstWrdsSoFar.Clear();
            foreach (var wrd in this._lstFoundWordsSoFar.OrderBy(p => p.word))
            {
                var tb = new TextBlock() { Text = wrd.word, FontSize = 12 };
                switch (wrd.foundStringType)
                {
                    case FoundWordType.FoundWordTypeSubWordInGrid:
                        break;
                    case FoundWordType.FoundWordTypeSubWordNotInGrid:
                        tb.Background = Brushes.LightBlue;
                        break;
                }
                this.mainWindow.LstWrdsSoFar.Add(tb);
            }
        }
    }
    public class LetterWheelLetter : Border
    {
        internal char ltr;
        public bool IsSelected;

        /*
<Border Grid.Row="1" CornerRadius="45" Height="50" Width="50">
<TextBlock Text="D" FontSize="40" Foreground="Black" HorizontalAlignment="Center" VerticalAlignment="Center"/>
</Border>

<Border Grid.Row="1" BorderThickness="1" BorderBrush="Black" Background="Green" CornerRadius="45" Height="50" Width="50">
<TextBlock Text="D" FontSize="40" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center"/>
</Border>
*/
        public LetterWheelLetter(char ltr)
        {
            this.ltr = ltr;
            //            this.BorderThickness = new System.Windows.Thickness(0);
            //            this.BorderBrush = Brushes.Black;
            this.CornerRadius = new System.Windows.CornerRadius(45);
            this.Height = 60;
            this.Width = 60;
            this.Background = Brushes.White;
            this.Child = new TextBlock()
            {
                Text = ltr.ToString(),
                FontSize = 50,
                IsHitTestVisible = false,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
        }

        public void Select()
        {
            if (!IsSelected)
            {
                var txtb = this.Child as TextBlock;
                txtb.Foreground = Brushes.White;
                this.Background = Brushes.Green;
                IsSelected = true;
            }
        }
        public void UnSelect()
        {
            if (IsSelected)
            {
                var txtb = this.Child as TextBlock;
                txtb.Foreground = Brushes.Black;
                this.Background = Brushes.Green;
                this.Background = Brushes.White;
                IsSelected = false;
            }
        }
        public override string ToString()
        {
            return $"{ltr}";
        }
    }
}