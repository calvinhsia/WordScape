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
            SubWordNotAWord,
            SubWordInGrid,
            SubWordNotInGrid,
            SubWordInLargeDictionary
        };
        class FoundWord
        {
            public FoundWordType foundStringType;
            public string word;
        }
        private WordScapeWindow wordScapeWindow;
        private WordContainer _wordCont;
        private GenGrid gridgen;
        int _WordsFound;

        private readonly List<LetterWheelLetter> _lstLetters = new List<LetterWheelLetter>();
        private readonly Polyline polyLine = new Polyline()
        {
            Stroke = Brushes.Red,
            StrokeThickness = 10,
            IsHitTestVisible = false
        };
        private bool _mouseIsDown = false;
        private bool _curlinefloating = false;
        private readonly List<LetterWheelLetter> _lstLtrsSelected = new List<LetterWheelLetter>();
        private readonly List<FoundWord> _lstFoundWordsSoFar = new List<FoundWord>();
        private Point _ptCirclePos = new Point(60, 10);
        private Point _ptCircleCtr;
        private readonly double _pctRadiusLettersInCircle = .7; // the letters (are in the within the circle, forming a smaller circle) are at this fraction of the circle radius
        private int NumLtrs => _wordCont.InitialWord.Length;

        public LetterWheel()
        {

        }
        public void LetterWheelInit(WordScapeWindow wordScapeWindow, WordContainer wordCont, GenGrid gridgen)
        {
            this.wordScapeWindow = wordScapeWindow;
            this._wordCont = wordCont;
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
            Canvas.SetLeft(circ, _ptCirclePos.X);
            Canvas.SetTop(circ, _ptCirclePos.Y);

            _ptCircleCtr = new Point(_ptCirclePos.X + circ.Width / 2, _ptCirclePos.Y + circ.Height / 2);
            this.Children.Add(circ);

            int ndx = 0;
            var radsPerLetter = (2 * Math.PI / NumLtrs);
            foreach (var ltr in _wordCont.InitialWord.ToUpper().OrderBy(p => gridgen._random.NextDouble()))
            //                foreach (var ltr in wordCont.InitialWord.ToUpper().OrderBy(p => Guid.NewGuid()))
            {
                var lett = new LetterWheelLetter(ltr);
                _lstLetters.Add(lett);

                var x = _ptCircleCtr.X + _pctRadiusLettersInCircle * circRadius * Math.Cos(radsPerLetter * ndx) - lett.Width / 2;
                var y = _ptCircleCtr.Y - _pctRadiusLettersInCircle * circRadius * Math.Sin(radsPerLetter * ndx) - lett.Height / 2;
                var letpt = new Point(x, y);
                Canvas.SetLeft(lett, letpt.X);
                Canvas.SetTop(lett, letpt.Y);
                this.Children.Add(lett);
                ndx++;
            }

            this.Children.Add(polyLine);
        }
        LetterWheelLetter LtrFromArgs(MouseEventArgs args)
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
                else if (x is Ellipse)
                {
                    if (!_mouseIsDown) // for the 1st mousedown, we'll allow a larger area
                    {
                        // calc x,y with circCtr as origin
                        var ptInCirc = new Point(pt.X - _ptCircleCtr.X, _ptCircleCtr.Y - pt.Y);

                        //var radsPerLetter = (2 * Math.PI / NumLtrs);
                        //var theta = Math.Atan2(ptInCirc.Y, ptInCirc.X);
                        //if (theta < 0)
                        //{
                        //    theta += 2 * Math.PI;
                        //}
                        //var ltrWheelNdx = (theta / (radsPerLetter));
                        //ltrUnderMouse = Children.OfType<LetterWheelLetter>().Skip((int)(ltrWheelNdx)).First();

                        LetterWheelLetter closestLetterWheelLetter = null;
                        var minDist = double.MaxValue;
                        foreach (var ltrWheelLtr in Children.OfType<LetterWheelLetter>())
                        {
                            var pttry = new Point(Canvas.GetLeft(ltrWheelLtr) + ltrWheelLtr.Width / 2 - _ptCircleCtr.X, _ptCircleCtr.Y - Canvas.GetTop(ltrWheelLtr) - ltrWheelLtr.Height / 2); // center of letter
                            var dist = Math.Sqrt(Math.Pow((pttry.X - ptInCirc.X), 2) + Math.Pow((pttry.Y - ptInCirc.Y), 2));
                            if (dist < minDist)
                            {
                                closestLetterWheelLetter = ltrWheelLtr;
                                minDist = dist;
                            }
                        }
                        if (minDist <= closestLetterWheelLetter.Height)
                        {
                            ltrUnderMouse = closestLetterWheelLetter;
                        }




                    }
                }
            }
            return ltrUnderMouse;
        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            polyLine.Points.Clear();
            _lstLtrsSelected.Clear();
            var ltrUnderMouse = LtrFromArgs(e);
            if (ltrUnderMouse != null)
            {
                ltrUnderMouse.Select();
                _lstLtrsSelected.Add(ltrUnderMouse);
                this.wordScapeWindow.StrWordSoFar = ltrUnderMouse.ltr.ToString();
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
                var ltrUnderMouse = LtrFromArgs(e);
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
                                this.wordScapeWindow.StrWordSoFar = this.wordScapeWindow.StrWordSoFar.Substring(0, _lstLtrsSelected.Count);
                            }
                        }
                    }
                    else
                    {
                        ltrUnderMouse.Select();
                        _lstLtrsSelected.Add(ltrUnderMouse);
                        this.wordScapeWindow.StrWordSoFar += ltrUnderMouse.ltr.ToString();
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
            var wrdSoFar = this.wordScapeWindow.StrWordSoFar;
            if (wrdSoFar.Length >= this.wordScapeWindow._wordGen._MinSubWordLen)
            {
                var doRefreshList = false;
                var foundWordType = FoundWordType.SubWordNotAWord;
                if (_lstFoundWordsSoFar.Where(p => p.word == wrdSoFar).Any()) // user already found this word
                {
                    foundWordType = FoundWordType.SubWordInGrid;
                }
                else
                {
                    var stat = this.ShowWord(wrdSoFar);
                    switch (stat)
                    {
                        case WordStatus.IsNotInGrid:
                            foundWordType = FoundWordType.SubWordNotInGrid;
                            if (!_wordCont.subwords.Contains(wrdSoFar))
                            {
                                if (this.wordScapeWindow._wordGen._dictionaryLibLarge.IsWord(wrdSoFar.ToLower()))
                                {
                                    foundWordType = FoundWordType.SubWordInLargeDictionary;
                                }
                                else
                                {
                                    foundWordType = FoundWordType.SubWordNotAWord;
                                }
                            }
                            _lstFoundWordsSoFar.Add(new FoundWord() { foundStringType = foundWordType, word = wrdSoFar });
                            doRefreshList = true;
                            break;
                        case WordStatus.IsAlreadyInGrid:
                            // animate word already found in grid
                            foundWordType = FoundWordType.SubWordInGrid;
                            break;
                        case WordStatus.IsShownInGridForFirstTime:
                            // animate word found in grid for first time
                            foundWordType = FoundWordType.SubWordInGrid;
                            _lstFoundWordsSoFar.Add(new FoundWord() { foundStringType = foundWordType, word = wrdSoFar });
                            _WordsFound++;
                            doRefreshList = true;
                            break;
                    }
                }
                if (doRefreshList)
                {
                    // animate new entry in list
                    this.RefreshWordList();
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
                this.wordScapeWindow.StrWordSoFar = "YAYYY!";
            }
            else
            {
                this.wordScapeWindow.StrWordSoFar = string.Empty;
            }
        }
        public enum WordStatus
        {
            IsAlreadyInGrid,
            IsShownInGridForFirstTime,
            IsNotInGrid
        }

        internal WordStatus ShowWord(string wrdSoFar)
        {
            var wrdStatus = WordStatus.IsNotInGrid;
            if (gridgen._dictPlacedWords.TryGetValue(wrdSoFar, out var ltrPlaced))
            {
                wrdStatus = WordStatus.IsAlreadyInGrid;
                int incx = 0, incy = 0, x = ltrPlaced.nX, y = ltrPlaced.nY;
                if (ltrPlaced.IsHoriz)
                {
                    incx = 1;
                }
                else
                {
                    incy = 1;
                }
                for (int i = 0; i < wrdSoFar.Length; i++)
                {
                    var ltrTile = this.wordScapeWindow.unigrid.Children[y * gridgen._MaxX + x] as LtrTile;
                    if (!ltrTile.IsShowing)
                    {
                        wrdStatus = WordStatus.IsShownInGridForFirstTime;
                        ltrTile.ShowLetter();
                    }
                    x += incx; y += incy;
                }
            }
            return wrdStatus;
        }

        void RefreshWordList()
        {
            this.wordScapeWindow.LstWrdsSoFar.Clear();
            foreach (var wrd in this._lstFoundWordsSoFar.OrderBy(p => p.word))
            {
                var tb = new TextBlock() { Text = wrd.word, FontSize = 12 };
                switch (wrd.foundStringType)
                {
                    case FoundWordType.SubWordNotAWord:
                        tb.Background = Brushes.LightPink;
                        break;
                    case FoundWordType.SubWordInLargeDictionary:
                        tb.Background = Brushes.LightSeaGreen;
                        break;
                    case FoundWordType.SubWordInGrid:
                        break;
                    case FoundWordType.SubWordNotInGrid:
                        tb.Background = Brushes.LightBlue;
                        break;
                }
                this.wordScapeWindow.LstWrdsSoFar.Add(tb);
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
            this.Height = 40;
            this.Width = 40;
            this.Background = Brushes.White;
            this.Child = new TextBlock()
            {
                Text = ltr.ToString(),
                FontSize = 40,
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