﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Graphics;
using Android.Views;
using Android.Widget;
using MusicApp.Resources.values;
using Org.Adw.Library.Widgets.Discreteseekbar;
using Square.Picasso;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicApp.Resources.Portable_Class
{
    [Activity(Label = "Player", Theme = "@style/Theme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Player : AppCompatActivity, Palette.IPaletteAsyncListener
    {
        public static Player instance;
        public Handler handler = new Handler();

        private DiscreteSeekBar bar;
        private ImageView imgView;
        private readonly int[] timers = new int[] { 0, 1, 10, 30, 60, 120 };
        private readonly string[] items = new string[] { "Off", "1 minute", "10 minutes", "30 minutes", "1 hour", "2 hours" };
        private int checkedItem = 0;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (MainActivity.Theme == 1)
                SetTheme(Resource.Style.DarkTheme);

            SetContentView(Resource.Layout.player);
            instance = this;
            CreatePlayer();
        }

        protected override void OnDestroy()
        {
            MainActivity.instance.ShowSmallPlayer();
            MainActivity.instance.PrepareSmallPlayer();
            base.OnDestroy();
            instance = null;
        }

        async void CreatePlayer()
        {
            while (MusicPlayer.CurrentID() == -1)
                await Task.Delay(100);

            Console.WriteLine("&CurrentID != -1");

            TextView title = FindViewById<TextView>(Resource.Id.playerTitle);
            TextView artist = FindViewById<TextView>(Resource.Id.playerArtist);
            imgView = FindViewById<ImageView>(Resource.Id.playerAlbum);

            FindViewById<ImageButton>(Resource.Id.lastButton).Click += Last_Click;
            FindViewById<ImageButton>(Resource.Id.playButton).Click += Play_Click;
            FindViewById<ImageButton>(Resource.Id.nextButton).Click += Next_Click;
            FindViewById<ImageButton>(Resource.Id.playerSleep).Click += SleepButton_Click;
            FindViewById<ImageButton>(Resource.Id.playerPlaylistAdd).Click += AddToPlaylist_Click; ;
            FindViewById<FloatingActionButton>(Resource.Id.downFAB).Click += Fab_Click;
            FindViewById<ImageButton>(Resource.Id.playerDownload).Click += Download_Click;
            FindViewById<ImageButton>(Resource.Id.playerYoutube).Click += Youtube_Click;

            Console.WriteLine("&ListenerAdded");

            Song current = MusicPlayer.queue[MusicPlayer.CurrentID()];

            title.Text = current.GetName();
            artist.Text = current.GetArtist();
            title.Selected = true;
            title.SetMarqueeRepeatLimit(3);
            artist.Selected = true;
            artist.SetMarqueeRepeatLimit(3);

            if (MusicPlayer.isRunning)
                FindViewById<ImageButton>(Resource.Id.playButton).SetImageResource(Resource.Drawable.ic_pause_black_24dp);
            else
                FindViewById<ImageButton>(Resource.Id.playButton).SetImageResource(Resource.Drawable.ic_play_arrow_black_24dp);

            if (current.GetAlbum() == null)
            {
                var songCover = Android.Net.Uri.Parse("content://media/external/audio/albumart");
                var songAlbumArtUri = ContentUris.WithAppendedId(songCover, current.GetAlbumArt());

                Picasso.With(this).Load(songAlbumArtUri).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(imgView);
            }
            else
            {
                Picasso.With(this).Load(current.GetAlbum()).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(imgView);
            }

            Palette.From(((BitmapDrawable)imgView.Drawable).Bitmap).MaximumColorCount(28).Generate(this);

            if (current.IsYt)
            {
                FindViewById<ImageButton>(Resource.Id.playerDownload).Visibility = ViewStates.Visible;
                FindViewById<ImageButton>(Resource.Id.playerYoutube).Visibility = ViewStates.Visible;
            }
            else
            {
                FindViewById<ImageButton>(Resource.Id.playerDownload).Visibility = ViewStates.Gone;
                FindViewById<ImageButton>(Resource.Id.playerYoutube).Visibility = ViewStates.Gone;
            }

            TextView NextTitle = FindViewById<TextView>(Resource.Id.nextTitle);
            TextView NextAlbum = FindViewById<TextView>(Resource.Id.nextArtist);
            Button ShowQueue = FindViewById<Button>(Resource.Id.showQueue);

            ShowQueue.Click += ShowQueue_Click;

            if (MainActivity.Theme == 1)
            {
                NextTitle.SetTextColor(Color.White);
                NextAlbum.SetTextColor(Color.White);
                NextAlbum.Alpha = 0.7f;
                ((GradientDrawable)ShowQueue.Background).SetStroke(5, Android.Content.Res.ColorStateList.ValueOf(Color.Argb(255, 62, 80, 180)));
                ShowQueue.SetTextColor(Color.Argb(255, 62, 80, 180));
            }
            else
            {
                ((GradientDrawable)ShowQueue.Background).SetStroke(5, Android.Content.Res.ColorStateList.ValueOf(Color.Argb(255, 21, 183, 237)));
                ShowQueue.SetTextColor(Color.Argb(255, 21, 183, 237));
            }

            bool asNext = MusicPlayer.queue.Count > MusicPlayer.CurrentID() + 1;
            if (asNext)
            {
                Song next = MusicPlayer.queue[MusicPlayer.CurrentID() + 1];
                NextTitle.Text = "Next music:";
                NextAlbum.Text = next.GetName();
                ImageView nextArt = FindViewById<ImageView>(Resource.Id.nextArt);

                if (next.GetAlbum() == null)
                {
                    var songCover = Android.Net.Uri.Parse("content://media/external/audio/albumart");
                    var nextAlbumArtUri = ContentUris.WithAppendedId(songCover, next.GetAlbumArt());

                    Picasso.With(this).Load(nextAlbumArtUri).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(nextArt);
                }
                else
                {
                    Picasso.With(this).Load(next.GetAlbum()).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(nextArt);
                }
            }
            else if (MusicPlayer.repeat)
            {
                Song next = MusicPlayer.queue[0];
                NextTitle.Text = "Next music:";
                NextAlbum.Text = next.GetName();
                ImageView nextArt = FindViewById<ImageView>(Resource.Id.nextArt);

                if (next.GetAlbum() == null)
                {
                    var songCover = Android.Net.Uri.Parse("content://media/external/audio/albumart");
                    var nextAlbumArtUri = ContentUris.WithAppendedId(songCover, next.GetAlbumArt());

                    Picasso.With(this).Load(nextAlbumArtUri).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(nextArt);
                }
                else
                {
                    Picasso.With(this).Load(next.GetAlbum()).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(nextArt);
                }
            }
            else
            {
                NextTitle.Text = "Next music:";
                NextAlbum.Text = "Nothing.";

                ImageView nextArt = FindViewById<ImageView>(Resource.Id.nextArt);
                Picasso.With(this).Load(Resource.Drawable.noAlbum).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(nextArt);
            }

            while (MusicPlayer.player == null || MusicPlayer.player.Duration == 0)
                await Task.Delay(100);

            bar = FindViewById<DiscreteSeekBar>(Resource.Id.songTimer);
            bar.SetNumericTransformer(new TimerTransformer());
            bar.LayoutParameters.Width = (int)(MainActivity.instance.Resources.DisplayMetrics.WidthPixels * 1.1f);
            MusicPlayer.SetSeekBar(bar);
            handler.PostDelayed(UpdateSeekBar, 1000);

            await Task.Delay(1000);
            MusicPlayer.SetSeekBar(bar);
        }

        public async void RefreshPlayer()
        {
            while (MusicPlayer.CurrentID() == -1)
                await Task.Delay(100);

            TextView title = FindViewById<TextView>(Resource.Id.playerTitle);
            TextView artist = FindViewById<TextView>(Resource.Id.playerArtist);
            imgView = FindViewById<ImageView>(Resource.Id.playerAlbum);

            Song current = MusicPlayer.queue[MusicPlayer.CurrentID()];

            title.Text = current.GetName();
            artist.Text = current.GetArtist();

            if (MusicPlayer.isRunning)
                FindViewById<ImageButton>(Resource.Id.playButton).SetImageResource(Resource.Drawable.ic_pause_black_24dp);
            else
                FindViewById<ImageButton>(Resource.Id.playButton).SetImageResource(Resource.Drawable.ic_play_arrow_black_24dp);

            if (!current.IsYt)
            {
                var songCover = Android.Net.Uri.Parse("content://media/external/audio/albumart");
                var songAlbumArtUri = ContentUris.WithAppendedId(songCover, current.GetAlbumArt());

                Picasso.With(this).Load(songAlbumArtUri).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(imgView);
            }
            else
            {
                Picasso.With(this).Load(current.GetAlbum()).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(imgView);
            }

            Palette.From(((BitmapDrawable)imgView.Drawable).Bitmap).MaximumColorCount(28).Generate(this);

            if (current.IsYt)
            {
                FindViewById<ImageButton>(Resource.Id.playerDownload).Visibility = ViewStates.Visible;
                FindViewById<ImageButton>(Resource.Id.playerYoutube).Visibility = ViewStates.Visible;
            }
            else
            {
                FindViewById<ImageButton>(Resource.Id.playerDownload).Visibility = ViewStates.Gone;
                FindViewById<ImageButton>(Resource.Id.playerYoutube).Visibility = ViewStates.Gone;
            }

            bool asNext = MusicPlayer.queue.Count > MusicPlayer.CurrentID() + 1;
            if (asNext)
            {
                Song next = MusicPlayer.queue[MusicPlayer.CurrentID() + 1];
                FindViewById<TextView>(Resource.Id.nextTitle).Text = "Next music:";
                FindViewById<TextView>(Resource.Id.nextArtist).Text = next.GetName();
                ImageView nextArt = FindViewById<ImageView>(Resource.Id.nextArt);

                if (next.GetAlbum() == null)
                {
                    var songCover = Android.Net.Uri.Parse("content://media/external/audio/albumart");
                    var nextAlbumArtUri = ContentUris.WithAppendedId(songCover, next.GetAlbumArt());

                    Picasso.With(this).Load(nextAlbumArtUri).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(nextArt);
                }
                else
                {
                    Picasso.With(this).Load(next.GetAlbum()).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(nextArt);
                }
            }
            else if (MusicPlayer.repeat)
            {
                Song next = MusicPlayer.queue[0];
                FindViewById<TextView>(Resource.Id.nextTitle).Text = "Next music:";
                FindViewById<TextView>(Resource.Id.nextArtist).Text = next.GetName();
                ImageView nextArt = FindViewById<ImageView>(Resource.Id.nextArt);

                if (next.GetAlbum() == null)
                {
                    var songCover = Android.Net.Uri.Parse("content://media/external/audio/albumart");
                    var nextAlbumArtUri = ContentUris.WithAppendedId(songCover, next.GetAlbumArt());

                    Picasso.With(this).Load(nextAlbumArtUri).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(nextArt);
                }
                else
                {
                    Picasso.With(this).Load(next.GetAlbum()).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(nextArt);
                }
            }
            else
            {
                FindViewById<TextView>(Resource.Id.nextTitle).Text = "Next music:";
                FindViewById<TextView>(Resource.Id.nextArtist).Text = "Nothing.";

                ImageView nextArt = FindViewById<ImageView>(Resource.Id.nextArt);
                Picasso.With(this).Load(Resource.Drawable.noAlbum).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(nextArt);
            }

            while (MusicPlayer.player.Duration < 1)
                await Task.Delay(100);

            bar.Progress = 0;
            bar.Max = (int)MusicPlayer.player.Duration;
        }

        public async void UpdateNext()
        {
            await Task.Delay(10);
            bool asNext = MusicPlayer.queue.Count > MusicPlayer.CurrentID() + 1;
            if (asNext)
            {
                Song next = MusicPlayer.queue[MusicPlayer.CurrentID() + 1];
                FindViewById<TextView>(Resource.Id.nextTitle).Text = "Next music:";
                FindViewById<TextView>(Resource.Id.nextArtist).Text = next.GetName();
                ImageView nextArt = FindViewById<ImageView>(Resource.Id.nextArt);

                if (next.GetAlbum() == null)
                {
                    var songCover = Android.Net.Uri.Parse("content://media/external/audio/albumart");
                    var nextAlbumArtUri = ContentUris.WithAppendedId(songCover, next.GetAlbumArt());

                    Picasso.With(this).Load(nextAlbumArtUri).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(nextArt);
                }
                else
                {
                    Picasso.With(this).Load(next.GetAlbum()).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(nextArt);
                }
            }
            else if (MusicPlayer.repeat)
            {
                Song next = MusicPlayer.queue[0];
                FindViewById<TextView>(Resource.Id.nextTitle).Text = "Next music:";
                FindViewById<TextView>(Resource.Id.nextArtist).Text = next.GetName();
                ImageView nextArt = FindViewById<ImageView>(Resource.Id.nextArt);

                if (next.GetAlbum() == null)
                {
                    var songCover = Android.Net.Uri.Parse("content://media/external/audio/albumart");
                    var nextAlbumArtUri = ContentUris.WithAppendedId(songCover, next.GetAlbumArt());

                    Picasso.With(this).Load(nextAlbumArtUri).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(nextArt);
                }
                else
                {
                    Picasso.With(this).Load(next.GetAlbum()).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(nextArt);
                }
            }
            else
            {
                FindViewById<TextView>(Resource.Id.nextTitle).Text = "Next music:";
                FindViewById<TextView>(Resource.Id.nextArtist).Text = "Nothing.";

                ImageView nextArt = FindViewById<ImageView>(Resource.Id.nextArt);
                Picasso.With(this).Load(Resource.Drawable.noAlbum).Placeholder(Resource.Drawable.MusicIcon).Resize(400, 400).CenterCrop().Into(nextArt);
            }
        }

        private void Download_Click(object sender, EventArgs e)
        {
            Song song = MusicPlayer.queue[MusicPlayer.CurrentID()];
            YoutubeEngine.Download(song.GetName(), song.youtubeID);
        }

        private void Youtube_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse("vnd.youtube://" + MusicPlayer.queue[MusicPlayer.CurrentID()].youtubeID));
            StartActivity(intent);
        }

        public void Stoped()
        {
            MainActivity.instance.FindViewById<BottomNavigationView>(Resource.Id.bottomView).SelectedItemId = Resource.Id.musicLayout;
        }

        private void AddToPlaylist_Click(object sender, EventArgs e)
        {
            Browse.act = this;
            Browse.inflater = LayoutInflater;
            Browse.GetPlaylist(MusicPlayer.queue[MusicPlayer.CurrentID()]);
        }

        public void UpdateSeekBar()
        {
            if (!MusicPlayer.isRunning)
            {
                handler.RemoveCallbacks(UpdateSeekBar);
                return;
            }

            bar.Progress = MusicPlayer.CurrentPosition;
            handler.PostDelayed(UpdateSeekBar, 1000);
        }

        private /*async*/ void Fab_Click(object sender, EventArgs e)
        {
            Finish();
            //MainActivity.instance.ToolBar.Visibility = ViewStates.Visible;
            //MainActivity.instance.FindViewById<BottomNavigationView>(Resource.Id.bottomView).Visibility = ViewStates.Visible;
            //MainActivity.instance.ShowSmallPlayer();
            //MainActivity.instance.PrepareSmallPlayer();
            //if (MainActivity.youtubeInstanceSave != null)
            //{
            //    int selectedTab = 0;
            //    switch (MainActivity.youtubeInstanceSave)
            //    {
            //        case "YoutubeEngine-All":
            //            selectedTab = 0;
            //            break;
            //        case "YoutubeEngine-Tracks":
            //            selectedTab = 1;
            //            break;
            //        case "YoutubeEngine-Playlists":
            //            selectedTab = 2;
            //            break;
            //        case "YoutubeEngine-Channels":
            //            selectedTab = 3;
            //            break;
            //        default:
            //            break;
            //    }
            //    await Task.Delay(10);
            //    MainActivity.instance.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.contentView, Pager.NewInstance(1, selectedTab)).Commit();
            //}
            //else
            //{
            //    MainActivity.instance.ResumeInstance();
            //}
        }

        private void ShowQueue_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(Queue));
            StartActivity(intent);
        }

        private void Last_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(MusicPlayer));
            intent.SetAction("Previus");
            StartService(intent);
        }

        private void Play_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(MusicPlayer));
            intent.SetAction("Pause");
            StartService(intent);
        }

        private void Next_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(MusicPlayer));
            intent.SetAction("Next");
            StartService(intent);
        }

        private void SleepButton_Click(object sender, EventArgs e)
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this, MainActivity.dialogTheme);
            builder.SetTitle("Sleep in :");
            builder.SetSingleChoiceItems(items, checkedItem, ((senders, eventargs) => { checkedItem = eventargs.Which; }));
            builder.SetPositiveButton("Ok", ((senders, args) => { Sleep(timers[checkedItem]); }));
            builder.SetNegativeButton("Cancel", ((senders, args) => { }));
            builder.Show();
        }

        void Sleep(int time)
        {
            Console.WriteLine("&Going to sleep in " + time + ", slected item is the " + checkedItem + " one.");
            Intent intent = new Intent(this, typeof(Sleeper));
            intent.PutExtra("time", time);
            this.StartService(intent);
        }

        protected override void OnResume()
        {
            base.OnResume();
            instance = this;
        }

        public void OnGenerated(Palette palette)
        {
            List<Palette.Swatch> swatches = palette.Swatches.OrderBy(x => x.Population).ToList();
            int i = swatches.Count - 1;
            Palette.Swatch swatch = palette.MutedSwatch;

            while(swatch == null)
            {
                swatch = swatches[i];
                i--;

                if (i == -1 && swatch == null)
                    return;
            }

            Color text = Color.Argb(Color.GetAlphaComponent(swatch.BodyTextColor), Color.GetRedComponent(swatch.BodyTextColor), Color.GetGreenComponent(swatch.BodyTextColor), Color.GetBlueComponent(swatch.BodyTextColor));
            Color background = Color.Argb(Color.GetAlphaComponent(swatch.Rgb), Color.GetRedComponent(swatch.Rgb), Color.GetGreenComponent(swatch.Rgb), Color.GetBlueComponent(swatch.Rgb));
            FindViewById<TextView>(Resource.Id.playerTitle).SetTextColor(text);
            FindViewById<TextView>(Resource.Id.playerArtist).SetTextColor(text);
            FindViewById<LinearLayout>(Resource.Id.infoPanel).SetBackgroundColor(background);
        }
    }
}