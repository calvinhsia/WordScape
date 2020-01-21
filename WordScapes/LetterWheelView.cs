using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using WordScape;

namespace WordScapes
{
    // https://stackoverflow.com/questions/3294590/set-the-absolute-position-of-a-view
    internal class LetterWheelLayout : RelativeLayout
    {

        List<LtrWheelLetterLayout> _lstLtrWheelLetterLayouts = new List<LtrWheelLetterLayout>();
        List<LtrWheelLetterLayout> _lstSelected = new List<LtrWheelLetterLayout>();
        private MainActivity _mainActivity;

        public LetterWheelLayout(MainActivity mainActivity) : base(mainActivity)
        {
            this._mainActivity = mainActivity;
            this.Touch += LetterWheelLayout_Touch;
        }

        private void LetterWheelLayout_Touch(object sender, TouchEventArgs e)
        {
            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    goto case MotionEventActions.Move;
                case MotionEventActions.Move:
                    var ltr = GetLetterFromTouch(e);
                    if (ltr != null)
                    {
                        if (!ltr._IsSelected)
                        {
                            ltr.Select();
                            _lstSelected.Add(ltr);
                            UpdateWordSofar();
                        }
                    }
                    break;
                case MotionEventActions.Up:
                    ClearSelection();
                    break;
            }
        }

        private void UpdateWordSofar()
        {
            var txtWordSoFar = string.Empty;
            foreach (var ltr in _lstSelected)
            {
                txtWordSoFar += ltr.textView.Text;
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
            _mainActivity._txtWordSoFar.Text = string.Empty;
        }

        private LtrWheelLetterLayout GetLetterFromTouch(TouchEventArgs e)
        {
            int[] locg = new int[2];
            this.GetLocationInWindow(locg);
            var X = (int)e.Event.RawX - locg[0];
            var Y = (int)e.Event.RawY - locg[1];
            LtrWheelLetterLayout ltrWheelLetterLayout = null;
            foreach (var ltr in _lstLtrWheelLetterLayouts)
            {
                if (IsPointInsideView(X, Y, ltr.textView))
                {
                    ltrWheelLetterLayout = ltr;
                    break;
                }
            }
            return ltrWheelLetterLayout;
        }

        private bool IsPointInsideView(float x, float y, View view)
        {
            var location = new int[2];
            view.GetLocationOnScreen(location);
            int viewX = location[0];

            //point is inside view bounds
            if (x > viewX && x < (viewX + view.Width))
            {
                return true;
            }

            return false;
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

            Paint red = new Paint
            {
                AntiAlias = true,
                Color = Color.Rgb(0xff, 0x44, 0x44)
            };
            red.StrokeWidth = 5;
            red.SetStyle(Paint.Style.Fill);
            var xx = new ShapeDrawable(new OvalShape());
            xx.Paint.Set(red);
            xx.SetBounds(0, 0, 1100, 1100);
            xx.Draw(canvas);
            float middle = canvas.Width * 0.25f;
            //canvas.DrawPaint(red);
            //            canvas.DrawRect(0, 0, middle, canvas.Height, green);
        }

        internal void CreateWheelLetters(MainActivity mainActivity)
        {
            this.RemoveAllViews();
            this._lstLtrWheelLetterLayouts.Clear();
            //            this.LayoutParameters = layoutParametersWheel;
            for (int i = 0; i < mainActivity._WordCont.InitialWord.Length; i++)
            {
                var wheelLetter = new LtrWheelLetterLayout(mainActivity, mainActivity._WordCont.InitialWord[i]);
                this._lstLtrWheelLetterLayouts.Add(wheelLetter);
                var layoutParametersWheel = new RelativeLayout.LayoutParams(100, 100);
                wheelLetter.LayoutParameters = layoutParametersWheel;
                layoutParametersWheel.LeftMargin = 10 + i * 100;
                layoutParametersWheel.TopMargin = 10 + i * 100;
                this.AddView(wheelLetter);
            }
        }
    }
    public class LtrWheelLetterLayout : RelativeLayout
    {
        public TextView textView;
        public bool _IsSelected = false;
        public LtrWheelLetterLayout(Context context, char letter) : base(context)
        {
            textView = new LtrWheelLetterAnd(context, letter);
            this.AddView(textView);
            Rect rect = new Rect();
            this.GetHitRect(rect);
            var location = new int[2];
            this.GetLocationOnScreen(location);
        }

        internal void Select()
        {
            if (!_IsSelected)
            {
                _IsSelected = !_IsSelected;
                this.SetBackgroundColor(Color.Purple);
                this.textView.SetTextColor(Color.White);
            }
        }
        internal void UnSelect()
        {
            if (_IsSelected)
            {
                _IsSelected = !_IsSelected;
                this.SetBackgroundColor(Color.White);
                this.textView.SetTextColor(Color.Black);
            }
        }

        public class LtrWheelLetterAnd : TextView
        {
            public LtrWheelLetterAnd(Context context, char letter) : base(context)
            {
                this.Text = letter.ToString();
                this.TextSize = 34;
                this.SetTypeface(null, TypefaceStyle.Bold);
                this.SetTextColor(Color.Black);
            }
        }
    }
}