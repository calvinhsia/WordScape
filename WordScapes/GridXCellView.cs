using Android.Content;
using Android.Graphics;
using Android.Widget;
using WordScape;

namespace WordScapes
{
    internal class GridXCellView : TextView
    {

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
            if (this._ltr == GenGrid.Blank)
            {
                this.Visibility = Android.Views.ViewStates.Invisible;
            }
            else
            {
                var fontSize = 15;
                if (gridgen._MaxX > 12)
                {
                    fontSize = 13;
                }
                SetBackgroundColor(Color.DarkCyan);
                SetTextColor(Color.White);
                this.Text = _ltr.ToString();
                this.TextSize = fontSize;
                this.SetTypeface(null, TypefaceStyle.Bold);
            }

        }
        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
        }
    }
}