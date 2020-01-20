using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Views;

namespace WordScapes
{
    // https://stackoverflow.com/questions/3294590/set-the-absolute-position-of-a-view
    internal class LetterWheelView : View
    {
        public LetterWheelView(Context context) : base(context)
        {
        }
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }
        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);
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
    }
}