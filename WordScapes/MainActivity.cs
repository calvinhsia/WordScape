using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
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
        TextView _txtTimer;
        TextView _txtWordSoFar;
        GridLayout _grdXWord;
        LetterWheelView _LetterWheelView;


        bool _timerEnabled = false;
        CancellationTokenSource _cts = null;
        int _nSecondsElapsed = 0;
        Task _tskTimer;

        public static MainActivity _instance;
        private Random _Random;
        public WordGenerator _wordGen;
        public WordContainer _WordCont;
        public GenGrid _gridgen;
        internal int _nCols = 12;
        internal int _nRows = 12;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            MainActivity._instance = this;
            _Random = new Random(
#if DEBUG
                    1
#endif
                    );
            createLayout();
            _wordGen = new WordGenerator(_Random);
            BtnNew_Click(_btnNew, null);
            //Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            //SetSupportActionBar(toolbar);

            //FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.Click += FabOnClick;
        }

        void createLayout()
        {
            WindowManager.DefaultDisplay.GetSize(_ptScreenSize);
            SetContentView(Resource.Layout.content_main);
            var layout = FindViewById<RelativeLayout>(Resource.Id.container);
            _txtTitle = FindViewById<TextView>(Resource.Id.textViewTitle);
            idTitleText = _txtTitle.Id;

            _txtTimer = new TextView(this)
            {
                Id = idtxtTimer,
                Text = "timer",
                TextSize = 30,
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

            _btnNew = new Button(this)
            {
                Id = idBtnPlayAgain,
                Text = "Play Again"
            };
            _btnNew.Click += BtnNew_Click;
            layout.AddView(_btnNew);


            _grdXWord = new GridLayout(this)
            {
                Id = idGrdXWord,
                ColumnCount = _nCols,
                RowCount = _nRows,
                Orientation= GridOrientation.Vertical,
                AlignmentMode = GridAlign.Bounds
            };
            //            _grdXWord.SetBackgroundColor(Color.Red);
            layout.AddView(_grdXWord);

            _txtWordSoFar = new TextView(this)
            {
                Id = idtxtWordSofar,
            };
            layout.AddView(_txtWordSoFar);

            _LetterWheelView = new LetterWheelView(this)
            {
                Id = idLtrWheelView,
                LayoutParameters = new ViewGroup.LayoutParams(_ptScreenSize.X, 200)
            };
            layout.AddView(_LetterWheelView);

            SetLayoutForOrientation(Android.Content.Res.Orientation.Portrait);
        }

        private void SetLayoutForOrientation(Android.Content.Res.Orientation orientation)
        {
            switch (orientation)
            {
                case Android.Content.Res.Orientation.Portrait:

                    _txtLenTargetWord.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_txtLenTargetWord.LayoutParameters)).AddRule(LayoutRules.Below, idTitleText);

                    _txtLenSubword.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_txtLenSubword.LayoutParameters)).AddRule(LayoutRules.RightOf, idtxtLenTargetWord);
                    ((RelativeLayout.LayoutParams)(_txtLenSubword.LayoutParameters)).AddRule(LayoutRules.Below, idTitleText);

                    _txtTimer.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_txtTimer.LayoutParameters)).AddRule(LayoutRules.LeftOf, idBtnPlayAgain);

                    _btnNew.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_btnNew.LayoutParameters)).AddRule(LayoutRules.AlignParentRight);

                    _grdXWord.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_grdXWord.LayoutParameters)).AddRule(LayoutRules.Below, idtxtLenTargetWord);



                    _txtWordSoFar.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_txtWordSoFar.LayoutParameters)).AddRule(LayoutRules.Below, idGrdXWord);


                    _LetterWheelView.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_LetterWheelView.LayoutParameters)).AddRule(LayoutRules.Below, idtxtWordSofar);


                    break;
            }
        }

        private async void BtnNew_Click(object sender, EventArgs e)
        {
            _btnNew.Enabled = false;
            if (_cts != null)
            {
                _cts.Cancel();
                _timerEnabled = false;
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
                    _WordCont = _wordGen.GenerateWord(LenTargetWord);
                    _gridgen = new GenGrid(maxX: 12, maxY: 12, _WordCont, this._Random);
                    _gridgen.Generate();
//                    var xx = _gridgen.ShowGrid();
                }
                catch (Exception ex)
                {
                    err = ex.ToString();
                }
            });
            _grdXWord.RemoveAllViews();
//            _grdXWord.Invalidate();
            _nCols = _gridgen._MaxX;
            _nRows = _gridgen._MaxY;
            _grdXWord.ColumnCount= _nCols;
            _grdXWord.RowCount = _nRows;
            if (string.IsNullOrEmpty(err))
            {
                this._txtWordSoFar.Text = _WordCont.InitialWord;
            }
            else
            {
                this._txtWordSoFar.Text = err;
            }
            DisplayXWords();
            _btnNew.Enabled = true;
        }

        private void DisplayXWords()
        {

            for (int x = 0; x < _nCols; x++)
            {
                for (int y = 0; y < _nRows; y++)
                {
                    var ltrTile = new GridXCellView(this, _gridgen,x, y);
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

