using Android.Content;
using Android.Graphics;
using Android.Widget;
using System;
using System.Threading.Tasks;
using WordScape;

namespace WordScapes
{
    public class GridXCellView : TextView
    {
        public bool IsShowing { get; internal set; }

        public const int margin = 1;
        public char _ltr;
        public GridXCellView(Context context, GenGrid gridgen, int x, int y) : base(context)
        {
            char ltr = gridgen._chars[x, y];
            this._ltr = ltr;
            var layoutParam = new GridLayout.LayoutParams();
            layoutParam.SetMargins(margin, margin, margin, margin);
            if (MainActivity._instance._ptScreenSize.X > MainActivity._instance._ptScreenSize.Y) //landscape
            {
                layoutParam.Width = MainActivity._instance._ptScreenSize.X / 2 / MainActivity._instance._nCols - 2 * margin;
            }
            else
            {
                layoutParam.Width = MainActivity._instance._ptScreenSize.X / MainActivity._instance._nCols - 2 * margin;
            }
            this.TextAlignment = Android.Views.TextAlignment.Center;
            this.LayoutParameters = layoutParam;
            if (this._ltr != GenGrid.Blank)
            {
                var fontSize = 14;
                if (gridgen._MaxX > 12)
                {
                    fontSize = 12;
                }
                SetBackgroundColor(Color.DarkCyan);
                SetTextColor(Color.White);
                this.TextSize = fontSize;
                this.SetTypeface(null, TypefaceStyle.Bold);
                this.Touch += GridXCellView_Touch;
            }
            else
            {
                this.Visibility = Android.Views.ViewStates.Invisible;
                IsShowing = true;
            }
        }

        private async void GridXCellView_Touch(object sender, TouchEventArgs e)
        {
            if (e.Event.Action == Android.Views.MotionEventActions.Down)
            {
                if (!IsShowing)
                {
                    MainActivity._instance.NumHintsUsed++;
                    MainActivity._instance._txtNumHintsUsed.Text = MainActivity._instance.NumHintsUsed.ToString();
                    Text = _ltr.ToString();
                    await Task.Delay(TimeSpan.FromSeconds(2.5));
                    if (!IsShowing) // after delay user could have got it.
                    {
                        Text = string.Empty;
                    }
                }
            }
        }

        internal void ShowLetter()
        {
            Text = _ltr.ToString();
            IsShowing = true;
        }
    }
}