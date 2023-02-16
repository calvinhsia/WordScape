using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using WordScape;

namespace WordScapes
{
    // https://stackoverflow.com/questions/3294590/set-the-absolute-position-of-a-view
    public class LetterWheelLayout : RelativeLayout
    {

        List<LtrWheelLetterLayout> _lstLtrWheelLetterLayouts = new List<LtrWheelLetterLayout>();
        List<LtrWheelLetterLayout> _lstSelected = new List<LtrWheelLetterLayout>();
        private MainActivity _mainActivity;
        Point _ptCircleCtr;
        int _circRadius = 480;
        private bool fDidLayout;
        private readonly double _pctRadiusLettersInCircle = .7; // the letters (are in the within the circle, forming a smaller circle) are at this fraction of the circle radius
        private readonly List<FoundWord> _lstFoundWordsSoFar = new List<FoundWord>();


        public LetterWheelLayout(MainActivity mainActivity) : base(mainActivity)
        {
            this._mainActivity = mainActivity;
            _circRadius = mainActivity._ptScreenSize.X / 3;
            this.Touch += LetterWheelLayout_Touch;
            //            this.SetBackgroundColor(Color.LightGray);
        }
        internal void CreateWheelLetters(MainActivity mainActivity, bool IsShuffling)
        {
            if (!IsShuffling)
            {
                _lstFoundWordsSoFar.Clear();
            }
            this.RemoveAllViews();

            fDidLayout = false;
            this._lstLtrWheelLetterLayouts.Clear();
            //            this.LayoutParameters = layoutParametersWheel;
            foreach (var ltr in mainActivity._wordCont.InitialWord.OrderBy(p => mainActivity._WordScapeOptions._Random.NextDouble()))
            {
                var wheelLetter = new LtrWheelLetterLayout(mainActivity, ltr);

                this._lstLtrWheelLetterLayouts.Add(wheelLetter);
                this.AddView(wheelLetter);
            }
        }
        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            if (!fDidLayout)
            {
                //                r = _mainActivity._ptScreenSize.X;
                Rect rect = new Rect(l, t, r, b);

                "rect = ({rect.Left},{ rect.Top}) ({rect.Right},{rect.Bottom})".ToString();
                _ptCircleCtr = new Point(rect.Left + (rect.Right - rect.Left) / 2, rect.Top + (rect.Bottom - rect.Top) / 2);
                if (_ptCircleCtr.X != 0 && _ptCircleCtr.Y != 0)
                {
                    fDidLayout = true;
                    //                this._mainActivity._txtWordSoFar.Text = _ptCircleCtr.ToString() + " " + rect.ToString();

                    //                    _circRadius = (rect.Right - rect.Left) / 2;
                    int ndx = 0;
                    var radsPerLetter = (2 * Math.PI / _lstLtrWheelLetterLayouts.Count);
                    foreach (var ltr in _lstLtrWheelLetterLayouts)
                    {
                        var x = _ptCircleCtr.X + _pctRadiusLettersInCircle * _circRadius * Math.Cos(radsPerLetter * ndx);// - ltr.Width / 2;
                        var y = _ptCircleCtr.Y - _pctRadiusLettersInCircle * _circRadius * Math.Sin(radsPerLetter * ndx);// - ltr.Height / 2;
                        var letpt = new Point((int)x, (int)y);
                        var layoutParametersWheel = new RelativeLayout.LayoutParams(ltr.Width, ltr.Height);
                        layoutParametersWheel.LeftMargin = letpt.X - rect.Left;// + _circRadius / 2 + 40;
                        layoutParametersWheel.TopMargin = letpt.Y - rect.Top + _circRadius / 2 + 40;
                        ltr.LayoutParameters = layoutParametersWheel;
                        ndx++;
                    }
                }
            }
        }

        private void LetterWheelLayout_Touch(object sender, TouchEventArgs e)
        {
            try
            {
                switch (e.Event.Action)
                {
                    case MotionEventActions.Down:
                        _mainActivity._txtWordSoFar.Text = string.Empty;
                        _mainActivity._txtWordSoFar.SetBackgroundColor(Color.White);
                        _mainActivity._txtWordSoFar.SetTextColor(Color.Black);
                        goto case MotionEventActions.Move;
                    case MotionEventActions.Move:
                        var ltr = GetLetterFromTouch(e);
                        if (ltr != null)
                        {
                            if (!ltr._IsSelected)
                            {
                                _lstSelected.Add(ltr);
                                UpdateWordSofar();
                            }
                            else
                            { // already in select list. Should we unselect?
                                if (_lstSelected.Count > 1)
                                {
                                    var at = _lstSelected.IndexOf(ltr);
                                    if (at == _lstSelected.Count - 2)
                                    {
                                        _lstSelected[_lstSelected.Count - 1].UnSelect();
                                        _lstSelected.RemoveAt(_lstSelected.Count - 1);
                                        UpdateWordSofar();
                                    }
                                }
                            }
                        }
                        else
                        { // ltr is null... are we out of the letter wheel view (with a tolerance)? if so, we'll unselect all
                            int[] locg = new int[2];
                            this.GetLocationOnScreen(locg);
                            var y = locg[1];
                            var tolerance = 40;
                            if (e.Event.RawY < y - tolerance || e.Event.RawY > y + this.Height + tolerance) // only deselect if Y out of bounds
                            {
                                ClearSelection();
                                UpdateWordSofar();
                            }
                        }
                        break;
                    case MotionEventActions.Up:
                        var wrdSoFar = _mainActivity._txtWordSoFar.Text;
                        if (wrdSoFar.Length >= _mainActivity._wordGen._MinSubWordLen)
                        {
                            var doRefreshList = false;
                            var foundWordType = FoundWordType.SubWordNotAWord;
                            if (_lstFoundWordsSoFar.Where(p => p.word == wrdSoFar).Any()) // user already found this word
                            {
                                foundWordType = FoundWordType.SubWordInGrid;
                                this.RefreshWordList(wrdSoFar);
                            }
                            else
                            {
                                var stat = this.ShowWord(wrdSoFar);
                                switch (stat)
                                {
                                    case WordStatus.IsNotInGrid:
                                        foundWordType = FoundWordType.SubWordNotInGrid;
                                        if (!_mainActivity._wordCont.subwords.Contains(wrdSoFar))
                                        {
                                            if (this._mainActivity._wordGen._dictionaryLibLarge.IsWord(wrdSoFar.ToLower()))
                                            {
                                                foundWordType = FoundWordType.SubWordInLargeDictionary;
                                            }
                                            else
                                            {
                                                foundWordType = FoundWordType.SubWordNotAWord;
                                            }
                                        }
                                        _lstFoundWordsSoFar.Add(new FoundWord() { foundWordType = foundWordType, word = wrdSoFar });
                                        doRefreshList = true;
                                        break;
                                    case WordStatus.IsAlreadyInGrid:
                                        foundWordType = FoundWordType.SubWordInGrid;
                                        break;
                                    case WordStatus.IsShownInGridForFirstTime:
                                        _mainActivity.NumWordsFound++;
                                        _mainActivity.UpdateScore();
                                        foundWordType = FoundWordType.SubWordInGrid;
                                        _lstFoundWordsSoFar.Add(new FoundWord() { foundWordType = foundWordType, word = wrdSoFar });
                                        //var anim = new ColorAnimation(fromValue:
                                        //    Colors.Black,
                                        //    toValue: Colors.Transparent,
                                        //    duration: TimeSpan.FromMilliseconds(100)
                                        //    )
                                        //{
                                        //    FillBehavior = FillBehavior.HoldEnd,
                                        //    RepeatBehavior = new RepeatBehavior(10)
                                        //};
                                        //WordScapeWindow.WordScapeWindowInstance.txtNumWordsFound.Background = new SolidColorBrush(Colors.Transparent);
                                        //WordScapeWindow.WordScapeWindowInstance.txtNumWordsFound.Background.BeginAnimation(SolidColorBrush.ColorProperty, anim);
                                        doRefreshList = true;
                                        break;
                                }
                            }
                            if (doRefreshList)
                            {
                                GetColorFromFoundWordType(foundWordType, out var forecolr, out var backColr);
                                this._mainActivity._txtWordSoFar.SetBackgroundColor(backColr);
                                this._mainActivity._txtWordSoFar.SetTextColor(forecolr);

                                this.RefreshWordList(wrdSoFar);
                            }
                        }
                        ClearSelection();
                        break;
                }
            }
            catch (Exception ex)
            {
                _mainActivity._txtWordSoFar.Text = ex.ToString();
            }
        }

        public static void GetColorFromFoundWordType(FoundWordType foundWordType, out Color forecolr, out Color backColr)
        {
            backColr = Color.Transparent;
            forecolr = Color.Black;
            switch (foundWordType)
            {
                case FoundWordType.SubWordNotAWord:
                    backColr = Color.LightPink;
                    break;
                case FoundWordType.SubWordInLargeDictionary:
                    backColr = Color.LightSeaGreen;
                    break;
                case FoundWordType.SubWordInGrid:
                    backColr = Color.DarkCyan;
                    forecolr = Color.White;
                    break;
                case FoundWordType.SubWordNotInGrid:
                    backColr = Color.LightBlue;
                    break;
            }
        }

        internal void Shuffle()
        {
            CreateWheelLetters(this._mainActivity, IsShuffling: true);
        }

        private WordStatus ShowWord(string wrdSoFar)
        {
            var wrdStatus = WordStatus.IsNotInGrid;
            if (_mainActivity._gridGen._dictPlacedWords.TryGetValue(wrdSoFar, out var ltrPlaced))
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
                    var ltrTile = this._mainActivity._grdXWord.GetChildAt(y * _mainActivity._gridGen._MaxX + x) as GridXCellView;
                    if (!ltrTile.IsShowing)
                    {
                        wrdStatus = WordStatus.IsShownInGridForFirstTime;
                        ltrTile.ShowLetter();
                    }
                    //ltrTile.txtBlock.Background = new SolidColorBrush(Colors.Transparent);
                    //ltrTile.txtBlock.Background.BeginAnimation(SolidColorBrush.ColorProperty, anim);



                    //var ppath = new PropertyPath("Background"); // new PropertyPath("Background")
                    //var sb = new Storyboard();
                    //sb.Children.Add(anim);

                    //Storyboard.SetTargetProperty(anim, ppath);
                    //Storyboard.SetTarget(sb, ltrTile.txtBlock);
                    //sb.Begin();
                    //ltrTile.txtBlock.BeginAnimation(ppath, anim);
                    //ltrTile.txtBlock.BeginAnimation()
                    //ltrTile.txtBlock.BeginAnimation(TextBlock.HeightProperty, anim);
                    //ltrTile.txtBlock.BeginAnimation(TextBlock.WidthProperty, anim);
                    x += incx; y += incy;
                }
            }
            return wrdStatus;
        }

        private void RefreshWordList(string wrdSoFar)
        {
            _mainActivity._ctrlWordList.SetWordList(_lstFoundWordsSoFar.OrderBy(p => p.word));
        }

        private void UpdateWordSofar()
        {
            var txtWordSoFar = string.Empty;
            foreach (var ltr in _lstSelected)
            {
                txtWordSoFar += ltr.textView.Text;
                ltr.Select(_lstSelected.Count >= _mainActivity._wordGen._MinSubWordLen ? LtrWheelLetterLayout.SelectLetterType.selLong : LtrWheelLetterLayout.SelectLetterType.selShort);
            }
            _mainActivity._txtWordSoFar.Text = txtWordSoFar;
        }

        private void ClearSelection()
        {
            _lstSelected.Clear();
            foreach (var letr in _lstLtrWheelLetterLayouts)
            {
                letr.UnSelect();
            }
            // don't clear wordsofar yet because user can see what the word was entered
        }

        private LtrWheelLetterLayout GetLetterFromTouch(TouchEventArgs e)
        {
            LtrWheelLetterLayout ltrWheelLetterLayout = null;
            if (_mainActivity._btnNew.Enabled) // if we're not calculating the next board
            {
                int[] locg = new int[2];
                this.GetLocationInWindow(locg);
                var X = (int)e.Event.RawX;// - locg[0];
                var Y = (int)e.Event.RawY;// - locg[1];
                foreach (var ltr in _lstLtrWheelLetterLayouts)
                {
                    if (IsPointInsideView(X, Y, ltr))
                    {
                        ltrWheelLetterLayout = ltr;
                        break;
                    }
                }
            }
            return ltrWheelLetterLayout;
        }

        private bool IsPointInsideView(float x, float y, LtrWheelLetterLayout view)
        {
            var location = new int[2];
            //view.GetLocationOnScreen(location);
            //int viewX = location[0];
            //int viewY = location[1];
            this.GetLocationOnScreen(location);
            x -= location[0];
            y -= location[1];

            Rect rect = new Rect();
            view.GetHitRect(rect);
            //            rect = new Rect(viewX, viewY, viewX + view.Width, viewY + view.Height);

            if (_lstSelected.Count == 0)
            {// for 1st letter, allow a larger rect
             //                rect.Left -= 50; rect.Right += 50; rect.Bottom += 50; rect.Top -= 50;
            }
            var isin = rect.Contains((int)x, (int)y);
            //            isin = false;
            //var margx = (view.LayoutParameters as RelativeLayout.LayoutParams)?.LeftMargin;
            //var margy = (view.LayoutParameters as RelativeLayout.LayoutParams)?.TopMargin;

            ////this._mainActivity._txtWordSoFar.Text = $"view={view.textView.Text} rect={rect}  ({viewX},{viewY}) ({margx}, {margy})  ({view.Width},{view.Height})";
            ////point is inside view bounds
            //// xxz ({x}, {y}) {viewX},{viewY})  rect = {rect.Left},{ rect.Top}) ({rect.Right},{rect.Bottom})
            //if (x > viewX && x < (viewX + view.Width))
            //{
            //    if (y > viewY && y < viewY + view.Height)
            //    {
            //        return true;
            //    }
            //}
            return isin;
        }
        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            //Paint green = new Paint
            //{
            //    AntiAlias = true,
            //    Color = Color.Rgb(0x99, 0xcc, 0),
            //};
            //green.SetStyle(Paint.Style.Stroke);

            var circlePaint = new Paint
            {
                AntiAlias = true,
                Color = Color.Rgb(0xff, 0x44, 0x44)
            };
            circlePaint.StrokeWidth = 15;
            circlePaint.SetStyle(Paint.Style.Stroke);
            var xCircle = new ShapeDrawable(new OvalShape());
            xCircle.Paint.Set(circlePaint);
            Rect rect = new Rect();
            this.GetHitRect(rect);
            rect.Left = _ptCircleCtr.X - _circRadius + _circRadius / 2;
            rect.Top = _ptCircleCtr.Y - _circRadius + _circRadius / 2;
            rect.Right = rect.Left + 2 * _circRadius;
            rect.Bottom = rect.Top + 2 * _circRadius;


            xCircle.SetBounds(rect.Left, rect.Top, rect.Right, rect.Bottom);
            //            xCircle.SetBounds(0, 0, 1100, 1100);
            xCircle.Draw(canvas);
            float middle = canvas.Width * 0.25f;
            //canvas.DrawPaint(red);
            //            canvas.DrawRect(0, 0, middle, canvas.Height, green);
        }
    }
    public class LtrWheelLetterLayout : RelativeLayout
    {
        public enum SelectLetterType
        {
            selShort,
            selLong
        }
        public TextView textView;
        public bool _IsSelected = false;
        public LtrWheelLetterLayout(Context context, char letter) : base(context)
        {
            textView = new LtrWheelLetterAnd(context, letter);
            this.AddView(textView);
            //            this.SetBackgroundColor(Color.CornflowerBlue);
            var w = (int)(.135 * MainActivity._instance._ptScreenSize.X);
            var parms = new RelativeLayout.LayoutParams(w, w);
            //parms.AddRule(LayoutRules.CenterHorizontal);
            //parms.AddRule(LayoutRules.CenterVertical);
            //parms.TopMargin = -20;
            textView.LayoutParameters = parms;
            //Rect rect = new Rect();
            //this.GetHitRect(rect);
            //var location = new int[2];
            //this.GetLocationOnScreen(location);
        }

        internal void Select(SelectLetterType selectLetterType)
        {
            if (!_IsSelected)
            {
                _IsSelected = !_IsSelected;
            }
            switch (selectLetterType)
            {
                case SelectLetterType.selLong:
                    this.SetBackgroundColor(Color.LightGreen);
                    break;
                case SelectLetterType.selShort:
                    this.SetBackgroundColor(Color.LightPink);
                    break;
            }
            this.textView.SetTextColor(Color.White);
        }
        internal void UnSelect()
        {
            if (_IsSelected)
            {
                _IsSelected = !_IsSelected;
            }
            this.SetBackgroundColor(Color.White);
            this.textView.SetTextColor(Color.Black);
        }

        //        public class myshape : RoutingEffect
        //        {
        //            public myshape() 
        //            {
        ////                this.re
        //            }

        //            public override void Draw(Canvas canvas, Paint paint)
        //            {
        //                throw new NotImplementedException();
        //            }
        //        }
        public class LtrWheelLetterAnd : TextView
        {
            public LtrWheelLetterAnd(Context context, char letter) : base(context)
            {
                this.Text = letter.ToString();
                this.TextSize = 34;
                this.TextAlignment = TextAlignment.Center;
                this.SetTypeface(null, TypefaceStyle.Bold);
                this.SetTextColor(Color.Black);
            }
        }
    }
}