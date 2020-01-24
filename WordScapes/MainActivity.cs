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

namespace WordScapes
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        public Point _ptScreenSize = new Point(); // Samsung Galaxy 9Plus : X = 1440, Y = 2792   GetRealSize x=1440, y=2960

        public const string prefTargWorLen = "TargWordLen";
        public const string prefSubWordLen = "SubWordLen";

        public const int idtxtTimer = 10;
        public const int idtxtLenTargetWord = 20;
        public const int idtxtLenSubWord = 30;
        public const int idtxtScore = 35;
        public const int idBtnPlayAgain = 40;
        public const int idBtnShuffle = 50;
        public const int idGrdXWord = 60;
        public const int idtxtWordSofar = 70;
        public const int idLtrWheelView = 80;
        private int idTitleText;

        TextView _txtTitle;
        Button _btnNew;
        TextView _txtLenTargetWord;
        TextView _txtLenSubword;
        TextView _txtScore;
        TextView _txtTimer;
        public TextView _txtWordSoFar;
        public MyGridLayout _grdXWord;
        public Button _btnShuffle;
        LetterWheelLayout _LetterWheelView;
        GridLayout _gridLayoutLetterWheel;
        public WordListControl _ctrlWordList;



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
        internal int _nCols = 12;
        internal int _nRows = 12;

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

            _txtLenTargetWord = new EditText(this)
            {
                Id = idtxtLenTargetWord,
                Text = Xamarin.Essentials.Preferences.Get(prefTargWorLen, 7).ToString()
            };
            layout.AddView(_txtLenTargetWord);

            _txtLenSubword = new EditText(this)
            {
                Id = idtxtLenSubWord,
                Text = Xamarin.Essentials.Preferences.Get(prefSubWordLen, 5).ToString()
            };
            layout.AddView(_txtLenSubword);

            _grdXWord = new MyGridLayout(this)
            {
                Id = idGrdXWord,
                ColumnCount = _nCols,
                RowCount = _nRows,
                Orientation = GridOrientation.Horizontal,
                AlignmentMode = GridAlign.Bounds
            };
            //            _grdXWord.SetBackgroundColor(Color.Red);
            layout.AddView(_grdXWord);

            _txtWordSoFar = new TextView(this)
            {
                Id = idtxtWordSofar,
                TextSize = 16
            };
            _txtWordSoFar.Click += (o, e) =>
              {
                  try
                  {
                      var uri = Android.Net.Uri.Parse($@"https://www.merriam-webster.com/dictionary/{_txtWordSoFar.Text}");
                      var intent = new Intent(Intent.ActionView, uri);
                      StartActivity(intent);
                  }
                  catch (Exception)
                  {
                  }
              };

            layout.AddView(_txtWordSoFar);


            /// the letterwheel is a circle. want some buttons/ui on either side, so use a 3 col grid layout
            /// 
            var specRow0 = GridLayout.InvokeSpec(0); // 1 row
            var specRow1 = GridLayout.InvokeSpec(0); // 1st row
            var specRow2 = GridLayout.InvokeSpec(0); // 1st row
            var specCol0 = GridLayout.InvokeSpec(0, GridLayout.LeftAlighment);
            var specCol1 = GridLayout.InvokeSpec(1, GridLayout.Center);
            var specCol2 = GridLayout.InvokeSpec(2, GridLayout.RightAlighment);
            _gridLayoutLetterWheel = new GridLayout(this)
            {
                Id = idLtrWheelView,
                RowCount = 1,
                ColumnCount = 3
            };
            layout.AddView(_gridLayoutLetterWheel);

            var linearLayoutCol0 = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical,
            };

            _txtScore = new TextView(this)
            {
                Id = idtxtScore
            };
            _txtScore.LayoutParameters = new LinearLayout.LayoutParams(250, LinearLayout.LayoutParams.MatchParent);
            linearLayoutCol0.AddView(_txtScore);

            _btnShuffle = new Button(this)
            {
                Text = "Shuffle",
                TextSize = 8
            };
            linearLayoutCol0.AddView(_btnShuffle);

            _gridLayoutLetterWheel.AddView(linearLayoutCol0, new GridLayout.LayoutParams(specRow0, specCol0));


            _LetterWheelView = new LetterWheelLayout(this)
            {
                LayoutParameters = new LinearLayout.LayoutParams(_ptScreenSize.X, 100)
            };
            _gridLayoutLetterWheel.AddView(_LetterWheelView, new GridLayout.LayoutParams(specRow1, specCol1));
            _btnShuffle.Click += (o, e) =>
              {
                  _LetterWheelView.Shuffle();
              };

            _btnNew = new Button(this)
            {
                Id = idBtnPlayAgain,
                Text = "Again",
                TextSize = 12
            };
            _btnNew.Click += BtnNew_Click;
            _gridLayoutLetterWheel.AddView(_btnNew, new GridLayout.LayoutParams(specRow2, specCol2));



            _ctrlWordList = new WordListControl(this);
            {
            };
            _ctrlWordList.AddTestWords();
            layout.AddView(_ctrlWordList);


            SetLayoutForOrientation(Android.Content.Res.Orientation.Portrait);
        }

        private void SetLayoutForOrientation(Android.Content.Res.Orientation orientation)
        {
            switch (orientation)
            {
                case Android.Content.Res.Orientation.Portrait:

                    _txtLenTargetWord.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_txtLenTargetWord.LayoutParameters)).AddRule(LayoutRules.RightOf, idTitleText);

                    _txtLenSubword.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_txtLenSubword.LayoutParameters)).AddRule(LayoutRules.RightOf, idtxtLenTargetWord);



                    _txtTimer.LayoutParameters = new RelativeLayout.LayoutParams(300, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_txtTimer.LayoutParameters)).AddRule(LayoutRules.AlignParentRight);

                    //                    _btnNew.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    //                    ((RelativeLayout.LayoutParams)(_btnNew.LayoutParameters)).AddRule(LayoutRules.AlignParentRight);

                    _grdXWord.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_grdXWord.LayoutParameters)).AddRule(LayoutRules.Below, idtxtLenTargetWord);



                    _txtWordSoFar.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_txtWordSoFar.LayoutParameters)).TopMargin = 30;
                    ((RelativeLayout.LayoutParams)(_txtWordSoFar.LayoutParameters)).AddRule(LayoutRules.Below, idGrdXWord);
                    ((RelativeLayout.LayoutParams)(_txtWordSoFar.LayoutParameters)).AddRule(LayoutRules.CenterHorizontal);



                    //                    _txtScore.LayoutParameters = new GridLayout.LayoutParams(200, RelativeLayout.LayoutParams.WrapContent);

                    _gridLayoutLetterWheel.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_gridLayoutLetterWheel.LayoutParameters)).AddRule(LayoutRules.Below, idtxtWordSofar);


                    _ctrlWordList.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.MatchParent);
                    ((RelativeLayout.LayoutParams)(_ctrlWordList.LayoutParameters)).AddRule(LayoutRules.Below, idLtrWheelView);


                    break;
            }
        }

        public void UpdateScore()
        {
            _txtScore.Text = $"{NumWordsFound}/{_gridgen.NumWordsPlaced}";
        }

        private async void BtnNew_Click(object sender, EventArgs e)
        {
            _btnNew.Enabled = false;
            _txtWordSoFar.Text = string.Empty;
            _txtScore.Text = string.Empty;
            NumWordsFound = 0;
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
            _grdXWord.ClearViews();
            _nCols = _gridgen._MaxX;
            _nRows = _gridgen._MaxY;
            _grdXWord.ColumnCount = _nCols;
            _grdXWord.RowCount = _nRows;

            DisplayXWords();
            UpdateScore();
            _ctrlWordList.AddTestWords();
            _LetterWheelView.CreateWheelLetters(this);

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

