using Android.Content;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace WordScapes
{
    public class MyGridLayout: GridLayout
    {
        public List<View> _lstViews = new List<View>();
        public MyGridLayout(Context context):base(context)
        {

        }
        public void ClearViews()
        {
            _lstViews.Clear();
            RemoveAllViews();
        }
        public override void AddView(View child)
        {
            base.AddView(child);
            _lstViews.Add(child);
        }
    }
}