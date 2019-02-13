﻿using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using MusicApp.Resources.values;
using Square.Picasso;
using System;
using System.Collections.Generic;

namespace MusicApp.Resources.Portable_Class
{
    public class QueueAdapter : RecyclerView.Adapter, IItemTouchAdapter
    {
        public event EventHandler<int> ItemClick;
        public event EventHandler<int> ItemLongCLick;

        public QueueAdapter(List<Song> songList) { }

        public override int ItemCount => MusicPlayer.UseCastPlayer ? MusicPlayer.RemotePlayer.MediaQueue.ItemCount + 1 : MusicPlayer.queue.Count + 1;

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            if (position + 1 == ItemCount)
            {
                QueueFooter holder = (QueueFooter)viewHolder;
                holder.SwitchButton.Checked = MusicPlayer.useAutoPlay;

                if ((MusicPlayer.CurrentID() == ItemCount - 2 || MusicPlayer.CurrentID() == ItemCount - 3) && MusicPlayer.useAutoPlay)
                {
                    holder.Autoplay.Visibility = ViewStates.Visible;

                    if (!holder.ItemView.HasOnClickListeners)
                        holder.ItemView.Click += (sender, eventArg) => 
                        {
                            Intent intent = new Intent(MainActivity.instance, typeof(MusicPlayer));
                            intent.SetAction("Next");
                            MainActivity.instance.StartService(intent);
                        };

                    if(MusicPlayer.autoPlay.Count > 0)
                    {
                        holder.RightIcon.Visibility = ViewStates.Visible;

                        if (MainActivity.Theme != 1)
                            holder.RightIcon.ImageTintList = ColorStateList.ValueOf(Color.Black);

                        Song ap = MusicPlayer.autoPlay[0];
                        holder.NextTitle.Text = ap.Title;

                        if (MainActivity.Theme == 1)
                            holder.NextTitle.SetTextColor(Color.White);
                        else
                            holder.NextTitle.SetTextColor(Color.Black);

                        holder.NextTitle.SetBackgroundResource(Android.Resource.Color.Transparent);
                        if (ap.IsYt)
                            Picasso.With(MainActivity.instance).Load(ap.Album).Placeholder(Resource.Drawable.noAlbum).Transform(new RemoveBlackBorder(true)).Into(holder.NextAlbum);
                        else
                        {
                            var songCover = Android.Net.Uri.Parse("content://media/external/audio/albumart");
                            var nextAlbumArtUri = ContentUris.WithAppendedId(songCover, ap.AlbumArt);

                            Picasso.With(MainActivity.instance).Load(nextAlbumArtUri).Placeholder(Resource.Drawable.noAlbum).Resize(400, 400).CenterCrop().Into(holder.NextAlbum);
                        }
                    }
                    else
                    {
                        int count = new Random().Next(6, 15);
                        string title = "a";
                        while(count > 0)
                        {
                            title += "a";
                            count--;
                        }

                        holder.NextTitle.Text = title;
                        holder.NextTitle.SetTextColor(Color.Transparent);
                        holder.NextTitle.SetBackgroundResource(Resource.Color.background_material_dark);
                        holder.NextAlbum.SetImageResource(Resource.Color.background_material_dark);
                        holder.RightIcon.Visibility = ViewStates.Gone;
                    }
                }
                else
                    holder.Autoplay.Visibility = ViewStates.Gone;

                if (!holder.SwitchButton.HasOnClickListeners)
                {
                    holder.SwitchButton.Click += (sender, e) =>
                    {
                        MusicPlayer.useAutoPlay = !MusicPlayer.useAutoPlay;
                        if (MusicPlayer.useAutoPlay)
                        {
                            MusicPlayer.repeat = false;
                            Queue.instance.menu.FindItem(Resource.Id.repeat).Icon.ClearColorFilter();
                        }
                    };

                    //holder.MixButton.Click += async (sender, e) =>
                    //{
                    //    YoutubeEngine.CreateMix(await MusicPlayer.GetItem());
                    //};
                }
            }
            else
            {
                RecyclerHolder holder = (RecyclerHolder)viewHolder;

                if (MainActivity.Theme == 1)
                {
                    holder.reorder.SetColorFilter(Color.White);
                    holder.Title.SetTextColor(Color.White);
                    holder.Artist.SetTextColor(Color.White);
                    holder.Artist.Alpha = 0.7f;
                    holder.youtubeIcon.SetColorFilter(Color.White);
                }

                holder.reorder.Visibility = ViewStates.Visible;
                holder.more.Visibility = ViewStates.Gone;
                if (position == MusicPlayer.CurrentID())
                {
                    holder.status.Visibility = ViewStates.Visible;
                    holder.status.Text = MusicPlayer.isRunning ? Queue.instance.GetString(Resource.String.playing) : Queue.instance.GetString(Resource.String.paused);
                    holder.status.SetTextColor(MusicPlayer.isRunning ? Color.Argb(255, 244, 81, 30) : Color.Argb(255, 66, 165, 245));
                }
                else
                    holder.status.Visibility = ViewStates.Gone;


                Song song = MusicPlayer.queue.Count <= position ? null : MusicPlayer.queue[position];
                if (song == null)
                {
                    if (holder.Title.Text.Length == 0)
                        holder.Title.Text = "aizquruhgqognbq";
                    if (holder.Artist.Text.Length == 0)
                        holder.Artist.Text = "ZJKGNZgzn";

                    holder.Title.SetTextColor(Color.Transparent);
                    holder.Title.SetBackgroundResource(Resource.Color.background_material_dark);
                    holder.Artist.SetTextColor(Color.Transparent);
                    holder.Artist.SetBackgroundResource(Resource.Color.background_material_dark);
                    holder.AlbumArt.SetImageResource(Resource.Color.background_material_dark);

                    MusicPlayer.RemotePlayer.MediaQueue.GetItemAtIndex(position);
                    return;
                }
                else
                {
                    holder.Title.SetBackgroundResource(0);
                    holder.Artist.SetBackgroundResource(0);
                }

                holder.Title.Text = song.Title;
                holder.Artist.Text = song.Artist;

                if (song.AlbumArt == -1 || song.IsYt)
                {
                    var songAlbumArtUri = Android.Net.Uri.Parse(song.Album);
                    Picasso.With(Application.Context).Load(songAlbumArtUri).Placeholder(Resource.Color.background_material_dark).Transform(new RemoveBlackBorder(true)).Into(holder.AlbumArt);
                }
                else
                {
                    var songCover = Android.Net.Uri.Parse("content://media/external/audio/albumart");
                    var songAlbumArtUri = ContentUris.WithAppendedId(songCover, song.AlbumArt);

                    Picasso.With(Application.Context).Load(songAlbumArtUri).Placeholder(Resource.Color.background_material_dark).Resize(400, 400).CenterCrop().Into(holder.AlbumArt);
                }

                if (song.IsLiveStream)
                    holder.Live.Visibility = ViewStates.Visible;
                else
                    holder.Live.Visibility = ViewStates.Gone;


                if (!holder.reorder.HasOnClickListeners)
                {
                    holder.reorder.Touch += (sender, e) =>
                    {
                        Queue.instance.itemTouchHelper.StartDrag(viewHolder);
                        MainActivity.instance.contentRefresh.Enabled = false;
                    };
                }

                if (song.IsParsed != true && song.IsYt)
                {
                    holder.youtubeIcon.SetImageResource(Resource.Drawable.needProcessing);
                    holder.youtubeIcon.Visibility = ViewStates.Visible;
                }
                else if (song.IsParsed == true && song.IsYt)
                {
                    holder.youtubeIcon.SetImageResource(Resource.Drawable.PublicIcon);
                    holder.youtubeIcon.Visibility = ViewStates.Visible;
                }
                else
                {
                    holder.youtubeIcon.Visibility = ViewStates.Gone;
                }
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Java.Lang.Object> payloads)
        {
            if (payloads.Count > 0)
            {
                if(payloads[0].ToString() == "UseAutoplay")
                {
                    QueueFooter holder = (QueueFooter)viewHolder;
                    holder.SwitchButton.Checked = MusicPlayer.useAutoPlay;

                    if (MusicPlayer.useAutoPlay && (MusicPlayer.CurrentID() == ItemCount - 2 || MusicPlayer.CurrentID() == ItemCount - 3))
                    {
                        holder.Autoplay.Visibility = ViewStates.Visible;

                        if (!holder.ItemView.HasOnClickListeners)
                            holder.ItemView.Click += (sender, eventArg) =>
                            {
                                Intent intent = new Intent(MainActivity.instance, typeof(MusicPlayer));
                                intent.SetAction("Next");
                                MainActivity.instance.StartService(intent);
                            };

                        if (MusicPlayer.autoPlay.Count > 0)
                        {
                            holder.RightIcon.Visibility = ViewStates.Visible;

                            if (MainActivity.Theme != 1)
                                holder.RightIcon.ImageTintList = ColorStateList.ValueOf(Color.Black);

                            Song ap = MusicPlayer.autoPlay[0];
                            holder.NextTitle.Text = ap.Title;

                            if (MainActivity.Theme == 1)
                                holder.NextTitle.SetTextColor(Color.White);
                            else
                                holder.NextTitle.SetTextColor(Color.Black);

                            holder.NextTitle.SetBackgroundResource(Android.Resource.Color.Transparent);
                            if (ap.IsYt)
                                Picasso.With(MainActivity.instance).Load(ap.Album).Placeholder(Resource.Drawable.noAlbum).Transform(new RemoveBlackBorder(true)).Into(holder.NextAlbum);
                            else
                            {
                                var songCover = Android.Net.Uri.Parse("content://media/external/audio/albumart");
                                var nextAlbumArtUri = ContentUris.WithAppendedId(songCover, ap.AlbumArt);

                                Picasso.With(MainActivity.instance).Load(nextAlbumArtUri).Placeholder(Resource.Drawable.noAlbum).Resize(400, 400).CenterCrop().Into(holder.NextAlbum);
                            }
                        }
                        else
                        {
                            int count = new Random().Next(6, 15);
                            string title = "a";
                            while (count > 0)
                            {
                                title += "a";
                                count--;
                            }

                            holder.NextTitle.Text = title;
                            holder.NextTitle.SetTextColor(Color.Transparent);
                            holder.NextTitle.SetBackgroundResource(Resource.Color.background_material_dark);
                            holder.NextAlbum.SetImageResource(Resource.Color.background_material_dark);
                            holder.RightIcon.Visibility = ViewStates.Gone;
                        }
                    }
                    else
                    {
                        holder.Autoplay.Visibility = ViewStates.Gone;
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("&Payload: " + payloads[0].ToString());
                    Console.WriteLine("&Position: " + position);
                    Console.WriteLine("&Song at position: " + MusicPlayer.queue[position]);

                    RecyclerHolder holder = (RecyclerHolder)viewHolder;

                    if (payloads[0].ToString() == holder.Title.Text)
                        return;

                    if (int.TryParse(payloads[0].ToString(), out int payload) && payload == Resource.Drawable.PublicIcon)
                    {
                        holder.youtubeIcon.SetImageResource(Resource.Drawable.PublicIcon);
                        return;
                    }

                    if (payloads[0].ToString() != null && (holder.Artist.Text == "" || holder.Artist.Text == null))
                    {
                        holder.Artist.Text = payloads[0].ToString();
                        Song song = MusicPlayer.queue[position];

                        if (song.IsYt)
                        {
                            var songAlbumArtUri = Android.Net.Uri.Parse(song.Album);
                            Picasso.With(Application.Context).Load(songAlbumArtUri).Placeholder(Resource.Color.background_material_dark).Transform(new RemoveBlackBorder(true)).Into(holder.AlbumArt);
                        }
                        return;
                    }
                }
            }

            base.OnBindViewHolder(viewHolder, position, payloads);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (viewType == 0)
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.SongList, parent, false);
                return new RecyclerHolder(itemView, OnClick, OnLongClick);
            }
            else
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.QueueFooter, parent, false);
                return new QueueFooter(itemView);
            }
        }

        public override int GetItemViewType(int position)
        {
            if (position + 1 == ItemCount)
                return 1;
            else
                return 0;
        }

        void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }

        void OnLongClick(int position)
        {
            ItemLongCLick?.Invoke(this, position);
        }

        public void ItemMoved(int fromPosition, int toPosition)
        {
            if(fromPosition < toPosition)
            {
                for(int i = fromPosition; i < toPosition; i++)
                    MusicPlayer.queue = Swap(MusicPlayer.queue, i, i + 1);
            }
            else
            {
                for(int i = fromPosition; i > toPosition; i--)
                    MusicPlayer.queue = Swap(MusicPlayer.queue, i, i - 1);
            }

            NotifyItemMoved(fromPosition, toPosition);
        }

        public void ItemMoveEnded(int fromPosition, int toPosition)
        {
            if (MusicPlayer.CurrentID() > fromPosition && MusicPlayer.CurrentID() <= toPosition)
                MusicPlayer.currentID--;

            else if (MusicPlayer.CurrentID() < fromPosition && MusicPlayer.CurrentID() >= toPosition)
                MusicPlayer.currentID++;

            else if (MusicPlayer.currentID == fromPosition)
                MusicPlayer.currentID = toPosition;

            if (MusicPlayer.UseCastPlayer)
            {
                int nextItemID = MusicPlayer.RemotePlayer.MediaQueue.ItemCount > toPosition ? MusicPlayer.RemotePlayer.MediaQueue.ItemIdAtIndex(toPosition + 1) : 0; //0 = InvalidItemID = end of the queue
                MusicPlayer.RemotePlayer.QueueReorderItems(new int[] { MusicPlayer.RemotePlayer.MediaQueue.ItemIdAtIndex(fromPosition) }, nextItemID, null);
            }
            MusicPlayer.UpdateQueueDataBase();
        }
        
        List<T> Swap<T>(List<T> list, int fromPosition, int toPosition)
        {
            T item = list[fromPosition];
            list[fromPosition] = list[toPosition];
            list[toPosition] = item;
            return list;
        }

        public void ItemDismissed(int position)
        {
            Song song = MusicPlayer.queue[position];
            Queue.RemoveFromQueue(position);
            Snackbar snackbar = Snackbar.Make(MainActivity.instance.FindViewById(Resource.Id.recycler), (song.Title.Length > 20 ? song.Title.Substring(0, 17) + "..." : song.Title) + Queue.instance.GetString(Resource.String.removed_from_queue), Snackbar.LengthShort)
                .SetAction(Queue.instance.GetString(Resource.String.undo), (view) =>
                {
                    Queue.InsertToQueue(position, song);
                    NotifyItemInserted(position);
                });
            snackbar.View.FindViewById<TextView>(Resource.Id.snackbar_text).SetTextColor(Color.White);
            snackbar.Show();
        }
    }
}