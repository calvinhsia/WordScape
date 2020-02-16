using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.Util.Prefs;
using WordScape;
//        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.KeyboardHidden, // prevent Activity restart,OnCreate called on Rotation

namespace WordScapes
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/AppTheme.NoActionBar",
                ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait,
        //        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.KeyboardHidden, // prevent Activity restart,OnCreate called on Rotation
        MainLauncher = true)]
    //    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        public Point _ptScreenSize = new Point(); // Samsung Galaxy 9Plus : X = 1440, Y = 2792   GetRealSize x=1440, y=2960

        public const string prefTargWorLen = "TargWordLen";
        public const string prefSubWordLen = "SubWordLen";
        public const string prefShowWordList = "ShowWordList";

        public const int idtxtTimer = 10;
        public const int idGrdXWord = 60;
        public const int idBorder = 65;
        public const int idtxtWordSofar = 70;
        public const int idLtrWheelView = 80;
        public const int idWordList = 80;
        private int idTitleText;

        TextView _txtTitle;
        public Button _btnNew;
        TextView _txtLenTargetWord;
        TextView _txtLenSubword;
        TextView _txtScore;
        TextView _txtTimer;
        public TextView _txtWordSoFar;
        public GridLayout _grdXWord;
        public Button _btnShuffle;
        LetterWheelLayout _LetterWheelView;
        LinearLayout _gridLayoutLetterWheel;
        public CheckBox _chkShowWordList;
        public WordListControl _ctrlWordList;
        public TextView _txtWordListLen;
        public TextView _txtNumHintsUsed;



        bool _timerEnabled = false;
        CancellationTokenSource _cts = null;
        int _nSecondsElapsed = 0;
        Task _tskTimer;

        public static MainActivity _instance;
        public Random _Random;
        public WordGenerator _wordGen;
        public WordContainer _wordCont;
        public GenGrid _gridgen;
        public int NumWordsFound;
        public int NumHintsUsed;
        internal int _nCols = 12;
        internal int _nRows = 12;
        private TextView _viewBorder;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            MainActivity._instance = this;
            _Random = new Random(
#if DEBUG
                    //                    1
#endif
                    );
            try
            {
                CreateLayout();
            }
            catch (Exception ex)
            {
                SetContentView(Resource.Layout.content_main);
                var layout = FindViewById<RelativeLayout>(Resource.Id.container);
                var textv = new TextView(this)
                {
                    Text = ex.ToString()
                };
                layout.AddView(textv);

                return;
            }
            _wordGen = new WordGenerator(_Random);
            BtnNew_Click(_btnNew, null);
            //Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            //SetSupportActionBar(toolbar);

            //FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.Click += FabOnClick;
        }

        void CreateLayout()
        {
            //            dp(Density Pixels), here's a formula to convert PXs from DPs:
            //(int)(< numberOfDPs > *getContext().getResources().getDisplayMetrics().density + 0.5f)
            WindowManager.DefaultDisplay.GetSize(_ptScreenSize);
            SetContentView(Resource.Layout.content_main);
            var layout = FindViewById<RelativeLayout>(Resource.Id.container);
            _txtTitle = FindViewById<TextView>(Resource.Id.textViewTitle);
            idTitleText = _txtTitle.Id;

            _txtTimer = new TextView(this)
            {
                Id = idtxtTimer,
                TextSize = 20,
                TextAlignment = TextAlignment.TextEnd
            };
            layout.AddView(_txtTimer);


            _grdXWord = new GridLayout(this)
            {
                Id = idGrdXWord,
                ColumnCount = _nCols,
                RowCount = _nRows,
                Orientation = GridOrientation.Horizontal,
                AlignmentMode = GridAlign.Bounds
            };
            //            _grdXWord.SetBackgroundColor(Color.Red);
            layout.AddView(_grdXWord);

            this._viewBorder = new TextView(this)
            {
                Id = idBorder,
            };
            _viewBorder.SetBackgroundColor(Color.Black);
            var bordLP = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 10);
            _viewBorder.LayoutParameters = bordLP;
            layout.AddView(_viewBorder);

            /// the letterwheel is a circle. want some buttons/ui on either side, so use a 3 col grid layout
            /// 
            _gridLayoutLetterWheel = new LinearLayout(this)
            {
                Id = idLtrWheelView,
                Orientation = Orientation.Horizontal,
            };
            layout.AddView(_gridLayoutLetterWheel);

            var linearLayoutCol0 = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical,
            };
            _gridLayoutLetterWheel.AddView(linearLayoutCol0, new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent));

            _btnShuffle = new Button(this)
            {
                Text = "Shuf",
                TextSize = 8,
                LayoutParameters = new LinearLayout.LayoutParams(200, LinearLayout.LayoutParams.WrapContent)
            };
            linearLayoutCol0.AddView(_btnShuffle);
            _btnShuffle.Click += (o, e) =>
            {
                _LetterWheelView.Shuffle();
            };

            _txtScore = new TextView(this)
            {
            };
            _txtScore.LayoutParameters = new LinearLayout.LayoutParams(200, LinearLayout.LayoutParams.MatchParent);
            linearLayoutCol0.AddView(_txtScore);

            _txtLenTargetWord = new EditText(this)
            {
                Text = Xamarin.Essentials.Preferences.Get(prefTargWorLen, 7).ToString(),
                TextSize = 14,
                LayoutParameters = new LinearLayout.LayoutParams(120, LinearLayout.LayoutParams.WrapContent)
            };
            linearLayoutCol0.AddView(_txtLenTargetWord);

            _txtLenSubword = new EditText(this)
            {
                Text = Xamarin.Essentials.Preferences.Get(prefSubWordLen, 5).ToString(),
                TextSize = 14,
                LayoutParameters = new LinearLayout.LayoutParams(120, LinearLayout.LayoutParams.WrapContent)
            };
            linearLayoutCol0.AddView(_txtLenSubword);



            _chkShowWordList = new CheckBox(this)
            {
                Text = "List",
                TextSize = 8,
                LayoutParameters = new LinearLayout.LayoutParams(200, LinearLayout.LayoutParams.WrapContent),
                Checked = Xamarin.Essentials.Preferences.Get(prefShowWordList, true)
            };
            _chkShowWordList.CheckedChange += (o, e) =>
            {
                Xamarin.Essentials.Preferences.Set(prefShowWordList, _chkShowWordList.Checked);
                _ctrlWordList.Visibility = _chkShowWordList.Checked ? ViewStates.Visible : ViewStates.Invisible;
                _txtWordListLen.Visibility = _chkShowWordList.Checked ? ViewStates.Visible : ViewStates.Invisible;
            };
            linearLayoutCol0.AddView(_chkShowWordList);

            _txtWordListLen = new TextView(this)
            {
                TextSize = 12,
                LayoutParameters = new LinearLayout.LayoutParams(100, LinearLayout.LayoutParams.WrapContent),
            };
            linearLayoutCol0.AddView(_txtWordListLen);

            var linearLayouCol2 = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };
            var lininside = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };
            _txtWordSoFar = new TextView(this)
            {
                Id = idtxtWordSofar,
                TextSize = 15,
                TextAlignment = TextAlignment.Center
            };
            _txtWordSoFar.Click += (o, e) =>
            {
                DoLookupOnlineDictionary(_txtWordSoFar.Text);
            };
            //            _txtWordSoFar.SetForegroundGravity(GravityFlags.CenterHorizontal);
            lininside.AddView(_txtWordSoFar);
            var p = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.MatchParent);
            p.Gravity = GravityFlags.Center;
            p.LeftMargin = 150;
            p.TopMargin = 15;
            lininside.LayoutParameters = p;
            linearLayouCol2.AddView(lininside);
            _LetterWheelView = new LetterWheelLayout(this)
            {
                LayoutParameters = new LinearLayout.LayoutParams((int)(0.59 * _ptScreenSize.X), LinearLayout.LayoutParams.WrapContent)
            };
            _LetterWheelView.SetGravity(GravityFlags.Center);
            linearLayouCol2.AddView(_LetterWheelView);
            _gridLayoutLetterWheel.AddView(linearLayouCol2);

            var linearLayoutCol3 = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical,
            };
            var layoutpCol3 = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
            layoutpCol3.LeftMargin = 100;
            _gridLayoutLetterWheel.AddView(linearLayoutCol3, layoutpCol3);

            _btnNew = new Button(this)
            {
                Text = "New",
                TextSize = 8,
                LayoutParameters = new LinearLayout.LayoutParams(100, LinearLayout.LayoutParams.WrapContent),
            };
            _btnNew.Click += BtnNew_Click;
            linearLayoutCol3.AddView(_btnNew, new LinearLayout.LayoutParams(200, LinearLayout.LayoutParams.WrapContent));

            _txtNumHintsUsed = new TextView(this)
            {
                TextSize = 12,
                LayoutParameters = new LinearLayout.LayoutParams(100, LinearLayout.LayoutParams.WrapContent),
            };
            linearLayoutCol3.AddView(_txtNumHintsUsed);

            _ctrlWordList = new WordListControl(this);
            {
            };
            //            _ctrlWordList.AddTestWords();
            layout.AddView(_ctrlWordList);


            SetLayoutForOrientation(Android.Content.Res.Orientation.Portrait);
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            //WindowManager.DefaultDisplay.GetSize(_ptScreenSize);
            //SetLayoutForOrientation(newConfig.Orientation);
        }


        public static void DoLookupOnlineDictionary(string text)
        {
            try
            {
                var uri = Android.Net.Uri.Parse($@"https://www.merriam-webster.com/dictionary/{text}");
                var intent = new Intent(Intent.ActionView, uri);
                MainActivity._instance.StartActivity(intent);
            }
            catch (Exception)
            {
            }
        }

        private void SetLayoutForOrientation(Android.Content.Res.Orientation orientation)
        {
            switch (orientation)
            {
                case Android.Content.Res.Orientation.Portrait:


                    _txtTimer.LayoutParameters = new RelativeLayout.LayoutParams(300, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_txtTimer.LayoutParameters)).AddRule(LayoutRules.AlignParentRight);

                    //                    _btnNew.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    //                    ((RelativeLayout.LayoutParams)(_btnNew.LayoutParameters)).AddRule(LayoutRules.AlignParentRight);

                    _grdXWord.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_grdXWord.LayoutParameters)).AddRule(LayoutRules.Below, idTitleText);



                    //_txtWordSoFar.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    //((RelativeLayout.LayoutParams)(_txtWordSoFar.LayoutParameters)).TopMargin = 30;
                    //((RelativeLayout.LayoutParams)(_txtWordSoFar.LayoutParameters)).AddRule(LayoutRules.Below, idGrdXWord);
                    //((RelativeLayout.LayoutParams)(_txtWordSoFar.LayoutParameters)).AddRule(LayoutRules.CenterHorizontal);



                    //                    _txtScore.LayoutParameters = new GridLayout.LayoutParams(200, RelativeLayout.LayoutParams.WrapContent);

                    ((RelativeLayout.LayoutParams)(_viewBorder.LayoutParameters)).AddRule(LayoutRules.Below, idGrdXWord);

                    _gridLayoutLetterWheel.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_gridLayoutLetterWheel.LayoutParameters)).AddRule(LayoutRules.Below, idBorder);


                    _ctrlWordList.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.MatchParent);
                    ((RelativeLayout.LayoutParams)(_ctrlWordList.LayoutParameters)).AddRule(LayoutRules.Below, idLtrWheelView);


                    break;
                case Android.Content.Res.Orientation.Landscape:
                    if (WindowManager.DefaultDisplay.Rotation == SurfaceOrientation.Rotation270) // tilted from portrait to right
                    {
                        _txtTimer.LayoutParameters = new RelativeLayout.LayoutParams(300, RelativeLayout.LayoutParams.WrapContent);
                        ((RelativeLayout.LayoutParams)(_txtTimer.LayoutParameters)).AddRule(LayoutRules.AlignParentRight);


                        _grdXWord.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                        ((RelativeLayout.LayoutParams)(_grdXWord.LayoutParameters)).AddRule(LayoutRules.Below, idTitleText);



                        _ctrlWordList.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.MatchParent);
                        ((RelativeLayout.LayoutParams)(_ctrlWordList.LayoutParameters)).AddRule(LayoutRules.RightOf, idGrdXWord);

                        _txtWordSoFar.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                        ((RelativeLayout.LayoutParams)(_txtWordSoFar.LayoutParameters)).TopMargin = 30;
                        ((RelativeLayout.LayoutParams)(_txtWordSoFar.LayoutParameters)).AddRule(LayoutRules.RightOf, idWordList);
                        ((RelativeLayout.LayoutParams)(_txtWordSoFar.LayoutParameters)).AddRule(LayoutRules.CenterHorizontal);


                        _gridLayoutLetterWheel.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
                        //((RelativeLayout.LayoutParams)(_gridLayoutLetterWheel.LayoutParameters)).AddRule(LayoutRules.RightOf, idWordList);
                        //((RelativeLayout.LayoutParams)(_gridLayoutLetterWheel.LayoutParameters)).AddRule(LayoutRules.Below, idtxtWordSofar);




                    }
                    else
                    {

                    }
                    break;
            }
        }

        public void UpdateScore()
        {
            _txtScore.Text = $"{NumWordsFound}/{_gridgen.NumWordsPlaced} {_wordCont.subwords.Count}";
            if (NumWordsFound == _gridgen.NumWordsPlaced)
            {
                _timerEnabled = false;
                var str = $"You Won in {_txtTimer.Text} # Hints used = {NumHintsUsed}";
                Android.Widget.Toast.MakeText(this, str, Android.Widget.ToastLength.Long).Show();
            }
        }

        private async void BtnNew_Click(object sender, EventArgs e)
        {
            _btnNew.Enabled = false;
            _txtWordSoFar.Text = string.Empty;
            NumWordsFound = 0;
            NumHintsUsed = 0;
            if (_cts != null)
            {
                _cts.Cancel();
                _timerEnabled = false;
            }
            int.TryParse(_txtLenTargetWord.Text, out var LenTargetWord);
            int.TryParse(_txtLenSubword.Text, out var minSubWordLen);
            Xamarin.Essentials.Preferences.Set(prefTargWorLen, LenTargetWord);
            Xamarin.Essentials.Preferences.Set(prefSubWordLen, minSubWordLen);

            _wordGen._MinSubWordLen = minSubWordLen;

            var err = string.Empty;
            await Task.Run(() =>
            {
                try
                {
                    _wordCont = _wordGen.GenerateWord(LenTargetWord);
                    _gridgen = new GenGrid(maxX: 12, maxY: 12, _wordCont, this._Random);
                    _gridgen.Generate();
                    //                    var xx = _gridgen.ShowGrid();
                }
                catch (Exception ex)
                {
                    err = ex.ToString();
                }
            });

            if (string.IsNullOrEmpty(err))
            {
            }
            else
            {
                this._txtWordSoFar.Text = err;
            }
            _txtScore.Text = string.Empty;
            _txtNumHintsUsed.Text = string.Empty;
            _grdXWord.RemoveAllViews();
            _ctrlWordList.SetWordList(lstWords: null);
            _nCols = _gridgen._MaxX;
            _nRows = _gridgen._MaxY;
            _grdXWord.ColumnCount = _nCols;
            _grdXWord.RowCount = _nRows;

            DisplayXWords();
            UpdateScore();
            _LetterWheelView.CreateWheelLetters(this, IsShuffling: false);

            if (_tskTimer != null)
            {
                await _tskTimer;
            }
            _cts = new CancellationTokenSource();
            _nSecondsElapsed = 0;
            _tskTimer = Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    RunOnUiThread(() =>
                    {
                        if (_timerEnabled)
                        {
                            _txtTimer.Text = $"{GetTimeAsString(_nSecondsElapsed)}";
                            _nSecondsElapsed++;
                        }
                    });
                    await Task.Delay(1000);
                }
            });

            _txtTimer.Text = string.Empty;
            _nSecondsElapsed = 0;
            _timerEnabled = true;

            _btnNew.Enabled = true;
        }

        private void DisplayXWords()
        {

            for (int y = 0; y < _nRows; y++)
            {
                for (int x = 0; x < _nCols; x++)
                {
                    var ltrTile = new GridXCellView(this, _gridgen, x, y);
                    _grdXWord.AddView(ltrTile);

                    //var x = new GridXCellView(this)
                    //{
                    //    Text = "A"
                    //};
                    //_grdXWord.AddView(x);
                }
            }
        }

        public static string GetTimeAsString(int tmpSecs)
        {
            var hrs = string.Empty;
            var mins = string.Empty;
            string secs;
            if (tmpSecs >= 3600)
            {
                hrs = $"{tmpSecs / 3600:n0}:";
                tmpSecs -= (tmpSecs / 3600) * 3600;
            }
            if (!string.IsNullOrEmpty(hrs) || tmpSecs >= 60)
            {
                mins = $"{(tmpSecs / 60).ToString(String.IsNullOrEmpty(hrs) ? "" : "00")}:";
                tmpSecs -= (tmpSecs / 60) * 60;
                secs = tmpSecs.ToString("00");
            }
            else
            {
                secs = tmpSecs.ToString();
            }
            return $"{hrs}{mins}{secs}";
        }
        public static (double X, double Y) GetScreenCoordinates<TRenderer>(TRenderer renderer) where TRenderer : Android.Views.View
        {
            double screenCoordinateX = renderer.GetX();
            double screenCoordinateY = renderer.GetY();
            Android.Views.IViewParent viewParent = renderer.Parent;
            while (viewParent != null)
            {
                if (viewParent is Android.Views.ViewGroup group)
                {
                    screenCoordinateX += group.GetX();
                    screenCoordinateY += group.GetY();
                    viewParent = group.Parent;
                }
                else
                {
                    viewParent = null;
                }
            }
            return (screenCoordinateX, screenCoordinateY);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

