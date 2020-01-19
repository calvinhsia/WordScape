using Android.Content;
using Android.Graphics;
using Android.Views;

namespace WordScapes
{
    internal class LetterWheelView : View
    {
        public LetterWheelView(Context context) : base(context)
        {
        }
        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            Paint green = new Paint
            {
                AntiAlias = true,
                Color = Color.Rgb(0x99, 0xcc, 0),
            };
            green.SetStyle(Paint.Style.FillAndStroke);

            Paint red = new Paint
            {
                AntiAlias = true,
                Color = Color.Rgb(0xff, 0x44, 0x44)
            };
            red.SetStyle(Paint.Style.FillAndStroke);

            float middle = canvas.Width * 0.25f;
            canvas.DrawPaint(red);
            canvas.DrawRect(0, 0, middle, canvas.Height, green);
        }
    }
}