﻿using Android.Content;
using Android.Views;
using Android.Widget;
using System.Collections;
using System.Collections.Generic;
using WordScape;
using Xamarin.Essentials;

namespace WordScapes
{
    public class WordListControl : HorizontalScrollView
    {

        MainActivity _mainActivity;
        GridLayout _grdWordList;
        public WordListControl(MainActivity mainActivity) : base(mainActivity)
        {
            this._mainActivity = mainActivity;
            var linearLayout = new LinearLayout(_mainActivity)
            {
                Id = MainActivity.idWordList,
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent),
                Orientation = Orientation.Horizontal
            };

            _grdWordList = new GridLayout(_mainActivity)
            {
                Orientation = GridOrientation.Vertical,
                //                ColumnCount = 15,
                RowCount = 5
            };
            //            linearLayout.AddView(_grdWordList);

            //for (int ndx = 0; ndx < 50; ndx++)
            //{
            //    var x = ndx / _grdWordList.RowCount;
            //    var y = ndx - x * _grdWordList.RowCount;
            //    var v = new TextView(_mainActivity)
            //    {
            //        Text = $"adsfadf{ndx} ",
            //        LayoutParameters = new GridLayout.LayoutParams(GridLayout.InvokeSpec(y), GridLayout.InvokeSpec(x))
            //    };

            //    _grdWordList.AddView(v);
            //}

            AddView(_grdWordList);
        }

        public void SetWordList(IEnumerable<FoundWord> lstWords)
        {
            _grdWordList.RemoveAllViews();
            int ndx = 0;
            if (lstWords != null && _mainActivity._chkShowWordList.Checked)
            {
                foreach (var item in lstWords)
                {
                    LetterWheelLayout.GetColorFromFoundWordType(item.foundWordType, out var forecolr, out var backColr);
                    var tv = new TextView(_mainActivity)
                    {
                        Text = item.word,
                    };
                    tv.SetBackgroundColor(backColr);
                    tv.SetTextColor(forecolr);
                    tv.Click += (o, e) =>
                    {
                        MainActivity.DoLookupOnlineDictionary(tv.Text);
                    };
                    var x = ndx / _grdWordList.RowCount;
                    var y = ndx - x * _grdWordList.RowCount;
                    var parms = new GridLayout.LayoutParams(GridLayout.InvokeSpec(y), GridLayout.InvokeSpec(x));
                    tv.LayoutParameters = parms;
                    parms.RightMargin = 5;
//                    ((GridLayout.LayoutParams)(tv.LayoutParameters)).RightMargin = 2;
                    _grdWordList.AddView(tv);
                    ndx++;
                }
            }
            _mainActivity._txtWordListLen.Text = ndx.ToString();
        }
    }
}