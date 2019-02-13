﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using Android.Widget;
using MusicApp;
using MusicApp.Resources.Portable_Class;
using MusicApp.Resources.values;
using Square.Picasso;
using System.Collections.Generic;
using Fragment = Android.Support.V4.App.Fragment;
using Toolbar = Android.Support.V7.Widget.Toolbar;

[Activity(Label = "Queue", Theme = "@style/Theme", ScreenOrientation = ScreenOrientation.Portrait)]
[Register("MusicApp/Queue")]
public class Queue : Fragment
{
    public static Queue instance;
    public RecyclerView ListView;
    public QueueAdapter adapter;
    public ItemTouchHelper itemTouchHelper;
    public IMenu menu;

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        View view = inflater.Inflate(Resource.Layout.RecyclerFragment, container, false);
        instance = this;
        ListView = view.FindViewById<RecyclerView>(Resource.Id.recycler);
        ListView.SetLayoutManager(new LinearLayoutManager(Application.Context));
        adapter = new QueueAdapter(MusicPlayer.queue);
        ListView.SetAdapter(adapter);
        adapter.ItemClick += ListView_ItemClick;
        adapter.ItemLongCLick += ListView_ItemLongCLick;
        ListView.SetItemAnimator(new DefaultItemAnimator());
        ListView.ScrollChange += Scroll;

        ItemTouchHelper.Callback callback = new ItemTouchCallback(adapter, true);
        itemTouchHelper = new ItemTouchHelper(callback);
        itemTouchHelper.AttachToRecyclerView(ListView);

        ListView.ScrollToPosition(MusicPlayer.CurrentID());

        if (MusicPlayer.UseCastPlayer)
        {
            Snackbar snackBar = Snackbar.Make(ListView, "Queue management with chromecast is currently in beta, expect some bugs.", (int)ToastLength.Short);
            snackBar.View.FindViewById<TextView>(Resource.Id.snackbar_text).SetTextColor(Color.White);
            snackBar.Show();
        }
        return view;
    }

    private void Scroll(object sender, View.ScrollChangeEventArgs e) { }

    //protected override void OnStop()
    //{
    //    Home.instance?.RefreshQueue();
    //    MusicPlayer.ParseNextSong();
    //    base.OnStop();
    //    instance = null;
    //}

    public void RefreshCurrent()
    {
        int first = ((LinearLayoutManager)ListView.GetLayoutManager()).FindFirstVisibleItemPosition();
        int last = ((LinearLayoutManager)ListView.GetLayoutManager()).FindLastVisibleItemPosition() - 1;
        for (int i = first; i <= last; i++)
        {
            Song song = MusicPlayer.queue[i];
            RecyclerHolder holder = (RecyclerHolder)ListView.GetChildViewHolder(((LinearLayoutManager)ListView.GetLayoutManager()).FindViewByPosition(i));
            if (MusicPlayer.queue[MusicPlayer.CurrentID()] == song)
            {
                holder.status.Text = MusicPlayer.isRunning ? GetString(Resource.String.playing) : GetString(Resource.String.paused);
                holder.status.SetTextColor(MusicPlayer.isRunning ? Color.Argb(255, 244, 81, 30) : Color.Argb(255, 66, 165, 245));
                holder.status.Visibility = ViewStates.Visible;
            }
            else
                holder.status.Visibility = ViewStates.Gone;
        }
    }

    public void RefreshAP()
    {
        adapter.NotifyItemChanged(MusicPlayer.queue.Count);
    }

    //public override bool OnCreateOptionsMenu(IMenu menu)
    //{
    //    MenuInflater.Inflate(Resource.Menu.QueueItems, menu);
    //    this.menu = menu;
    //    menu.FindItem(Resource.Id.shuffle).Icon.SetColorFilter(Color.White, PorterDuff.Mode.Multiply);
    //    if (MusicPlayer.repeat)
    //        menu.FindItem(Resource.Id.repeat).Icon.SetColorFilter(Color.Argb(255, 21, 183, 237), PorterDuff.Mode.Multiply);
            
    //    return base.OnCreateOptionsMenu(menu);
    //}

    //public override bool OnOptionsItemSelected(IMenuItem item)
    //{
    //    if (item.ItemId == Android.Resource.Id.Home)
    //    {
    //        Finish();
    //    }
    //    else if(item.ItemId == Resource.Id.shuffle)
    //    {
    //        ShuffleQueue();
    //    }
    //    else if(item.ItemId == Resource.Id.repeat)
    //    {
    //        Repeat(item);
    //    }
    //    return base.OnOptionsItemSelected(item);
    //}

    void ShuffleQueue()
    {
        Intent intent = new Intent(Activity, typeof(MusicPlayer));
        intent.SetAction("RandomizeQueue");
        Activity.StartService(intent);
    }

    void Repeat(IMenuItem item)
    {
        MusicPlayer.repeat = !MusicPlayer.repeat;

        if (MusicPlayer.UseCastPlayer)
            MusicPlayer.RemotePlayer.QueueSetRepeatMode(MusicPlayer.repeat ? 1 : 0, null);

        if (MusicPlayer.repeat)
        {
            item.Icon.SetColorFilter(Color.Argb(255, 21, 183, 237), PorterDuff.Mode.Multiply);
            MusicPlayer.useAutoPlay = false;
            adapter.NotifyItemChanged(adapter.ItemCount - 1, "UseAutoplay");
        }
        else
        {
            item.Icon.ClearColorFilter();
            MusicPlayer.useAutoPlay = true;
            adapter.NotifyItemChanged(adapter.ItemCount - 1, "UseAutoplay");
        }
    }

    private void ListView_ItemClick(object sender, int Position)
    {
        Song item = MusicPlayer.queue[Position];

        if (Position == MusicPlayer.CurrentID())
        {
            Intent intent = new Intent(Activity, typeof(MusicPlayer));
            intent.SetAction("Pause");
            Activity.StartService(intent);
        }
        else if(MusicPlayer.instance != null)
            MusicPlayer.instance.SwitchQueue(Position);
        else
        {
            Intent intent = new Intent(Activity, typeof(MusicPlayer));
            intent.SetAction("SwitchQueue");
            intent.PutExtra("queueSlot", Position);
            Activity.StartService(intent);
        }
    }

    private void ListView_ItemLongCLick(object sender, int position)
    {
        More(position);
    }

    public void More(int position)
    {
        Song item = MusicPlayer.queue[position];

        BottomSheetDialog bottomSheet = new BottomSheetDialog(MainActivity.instance);
        View bottomView = LayoutInflater.Inflate(Resource.Layout.BottomSheet, null);
        bottomView.FindViewById<TextView>(Resource.Id.bsTitle).Text = item.Title;
        bottomView.FindViewById<TextView>(Resource.Id.bsArtist).Text = item.Artist;
        if (item.Album == null)
        {
            var songCover = Android.Net.Uri.Parse("content://media/external/audio/albumart");
            var nextAlbumArtUri = ContentUris.WithAppendedId(songCover, item.AlbumArt);

            Picasso.With(MainActivity.instance).Load(nextAlbumArtUri).Placeholder(Resource.Drawable.noAlbum).Resize(400, 400).CenterCrop().Into(bottomView.FindViewById<ImageView>(Resource.Id.bsArt));
        }
        else
        {
            Picasso.With(MainActivity.instance).Load(item.Album).Placeholder(Resource.Drawable.noAlbum).Transform(new RemoveBlackBorder(true)).Into(bottomView.FindViewById<ImageView>(Resource.Id.bsArt));
        }
        bottomSheet.SetContentView(bottomView);

        List<BottomSheetAction> actions = new List<BottomSheetAction>
        {
            new BottomSheetAction(Resource.Drawable.Play, Resources.GetString(Resource.String.play), (sender, eventArg) => { ListView_ItemClick(null, position); bottomSheet.Dismiss(); }),
            new BottomSheetAction(Resource.Drawable.Close, Resources.GetString(Resource.String.remove_from_queue), (sender, eventArg) => { RemoveFromQueue(position); bottomSheet.Dismiss(); }),
            new BottomSheetAction(Resource.Drawable.PlaylistAdd, Resources.GetString(Resource.String.add_to_playlist), (sender, eventArg) => { Browse.GetPlaylist(item); bottomSheet.Dismiss(); })
        };

        if (item.IsYt)
        {
            actions.AddRange(new BottomSheetAction[]
            {
                new BottomSheetAction(Resource.Drawable.PlayCircle, Resources.GetString(Resource.String.create_mix_from_song), (sender, eventArg) =>
                {
                    YoutubeEngine.CreateMix(item);
                    bottomSheet.Dismiss();
                }),
                new BottomSheetAction(Resource.Drawable.Download, Resources.GetString(Resource.String.download), (sender, eventArg) =>
                {
                    YoutubeEngine.Download(item.Title, item.YoutubeID);
                    bottomSheet.Dismiss();
                })
            });
        }
        else
        {
            actions.Add(new BottomSheetAction(Resource.Drawable.Edit, Resources.GetString(Resource.String.edit_metadata), (sender, eventArg) =>
            {
                Browse.EditMetadata(item);
                bottomSheet.Dismiss();
            }));
        }

        bottomSheet.FindViewById<ListView>(Resource.Id.bsItems).Adapter = new BottomSheetAdapter(MainActivity.instance, Resource.Layout.BottomSheetText, actions);
        bottomSheet.Show();
    }

    public static void InsertToQueue(int position, Song item)
    {
        if (MusicPlayer.CurrentID() >= position)
        {
            MusicPlayer.currentID++;
            MusicPlayer.SaveQueueSlot();
        }

        MusicPlayer.InsertToQueue(position, item);
    }

    public static void RemoveFromQueue(int position)
    {
        if (MusicPlayer.CurrentID() > position)
        {
            MusicPlayer.currentID--;
            MusicPlayer.SaveQueueSlot();
        }

        MusicPlayer.RemoveFromQueue(position);

        if (instance != null)
            instance.adapter.NotifyItemRemoved(position);
    }

    public override void OnResume()
    {
        base.OnResume();
        instance = this;
    }
}