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

namespace WordScapes
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        public const int idtxtTimer = 10;

        public const int idBtnPlayAgain = 60;
        public const int idBtnShuffle = 70;
        TextView _txtTimer;
        Button _btnNew;

        void createLayout()
        {
            SetContentView(Resource.Layout.content_main);
            var layout = FindViewById<RelativeLayout>(Resource.Id.container);
            _btnNew = new Button(this)
            {
                Text = "Play Again"
            };
            _btnNew.Click += BtnNew_Click;
            layout.AddView(_btnNew);

            _txtTimer = new TextView(this)
            {
                Id = idtxtTimer,
                Text = "timer",
                TextSize = 30
            };
            layout.AddView(_txtTimer);
            SetLayoutForOrientation(Android.Content.Res.Orientation.Portrait);
        }

        private void SetLayoutForOrientation(Android.Content.Res.Orientation orientation)
        {
            switch (orientation)
            {
                case Android.Content.Res.Orientation.Portrait:
                    _txtTimer.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                    ((RelativeLayout.LayoutParams)(_txtTimer.LayoutParameters)).AddRule(LayoutRules.AlignParentRight);

                    break;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            createLayout();
            BtnNew_Click(_btnNew, null);
            //Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            //SetSupportActionBar(toolbar);

            //FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.Click += FabOnClick;
        }

        bool _timerEnabled = false;
        CancellationTokenSource _cts = null;
        int _nSecondsElapsed = 0;
        Task _tskTimer;
        private async void BtnNew_Click(object sender, EventArgs e)
        {
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

