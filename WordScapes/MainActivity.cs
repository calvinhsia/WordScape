using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using WordScape;

namespace WordScapes
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        public const int idBtnPlayAgain = 10;
        public const int idtxtLenTargetWord = 20;
        public const int idtxtLenSubWord = 30;
        public const int idtxtTimer = 40;
        public const int idBtnShuffle = 70;
        public const int idtxtWordSofar = 100;
        private int idTitleText;

        TextView _txtTitle;
        TextView _txtTimer;
        TextView _txtLenTargetWord;
        TextView _txtLenSubword;
        TextView _txtWordSoFar;
        Button _btnNew;

        bool _timerEnabled = false;
        CancellationTokenSource _cts = null;
        int _nSecondsElapsed = 0;
        Task _tskTimer;

        public static MainActivity _instance;
        private Random _Random;
        public WordGenerator _wordGen;
        public WordContainer _WordCont;
        public GenGrid _gridgen;

        void createLayout()
        {
            SetContentView(Resource.Layout.content_main);
            var layout = FindViewById<RelativeLayout>(Resource.Id.container);
            _txtTitle = FindViewById<TextView>(Resource.Id.textViewTitle);
            idTitleText = _txtTitle.Id;

            _btnNew = new Button(this)
            {
                Id = idBtnPlayAgain,
                Text = "Play Again"
            };
            _btnNew.Click += BtnNew_Click;
            layout.AddView(_btnNew);

            _txtLenTargetWord = new EditText(this)
            {
                Id = idtxtLenTargetWord,
                Text = "9"
            };
            layout.AddView(_txtLenTargetWord);

            _txtLenSubword = new EditText(this)
            {
                Id = idtxtLenSubWord,
                Text = "5"
            };
            layout.AddView(_txtLenSubword);

            _txtTimer = new TextView(this)
            {
                Id = idtxtTimer,
                Text = "timer",
                TextSize = 30,
            };
            layout.AddView(_txtTimer);

            _txtWordSoFar = new TextView(this)
            {
                Id = idtxtWordSofar,
                Text = "wsofar"
            };
            layout.AddView(_txtWordSoFar);

            SetLayoutForOrientation(Android.Content.Res.Orientation.Portrait);
        }

        private void SetLayoutForOrientation(Android.Content.Res.Orientation orientation)
        {
            switch (orientation)
            {
                case Android.Content.Res.Orientation.Portrait:
                    _btnNew.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_btnNew.LayoutParameters)).AddRule(LayoutRules.Below, idTitleText);

                    _txtLenTargetWord.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_txtLenTargetWord.LayoutParameters)).AddRule(LayoutRules.RightOf, idBtnPlayAgain);
                    ((RelativeLayout.LayoutParams)(_txtLenTargetWord.LayoutParameters)).AddRule(LayoutRules.Below, idTitleText);

                    _txtLenSubword.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_txtLenSubword.LayoutParameters)).AddRule(LayoutRules.RightOf, idtxtLenTargetWord);
                    ((RelativeLayout.LayoutParams)(_txtLenSubword.LayoutParameters)).AddRule(LayoutRules.Below, idTitleText);

                    _txtTimer.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_txtTimer.LayoutParameters)).AddRule(LayoutRules.AlignParentRight);




                    _txtWordSoFar.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_txtWordSoFar.LayoutParameters)).AddRule(LayoutRules.Below, idBtnPlayAgain);




                    break;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            MainActivity._instance = this;
            this._Random = new Random();
            createLayout();
            _wordGen = new WordGenerator(_Random);
            BtnNew_Click(_btnNew, null);
            //Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            //SetSupportActionBar(toolbar);

            //FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.Click += FabOnClick;
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
            _wordGen._MinSubWordLen = minSubWordLen;
            var err = string.Empty;
            await Task.Run(() =>
            {
                try
                {
                    _WordCont = _wordGen.GenerateWord(LenTargetWord);
                    _gridgen = new GenGrid(maxX: 12, maxY: 12, _WordCont, this._Random);
                }
                catch (Exception ex)
                {
                    err = ex.ToString();
                }
            });
            if (string.IsNullOrEmpty(err))
            {
                this._txtWordSoFar.Text = _WordCont.InitialWord;
            }
            else
            {
                this._txtWordSoFar.Text = err;
            }
            _btnNew.Enabled = true;
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

