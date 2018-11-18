﻿using Android.App;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;

namespace MusicApp.Resources.Portable_Class
{
    public class AddToPlaylistAdapter : RecyclerView.Adapter
    {
        private List<PlaylistItem> Playlists = new List<PlaylistItem>();
        public event EventHandler<int> ItemClick;

        public AddToPlaylistAdapter(List<PlaylistItem> Playlists)
        {
            this.Playlists = Playlists;
        }

        public override int ItemCount => Playlists.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            if (position == Playlists.Count - 1 && Playlists[position].Name == "Loading" && Playlists[position].LocalID == 0 && Playlists[position].YoutubeID == null)
                return;

            AddToPlaylistHolder holder = (AddToPlaylistHolder)viewHolder;
            holder.Title.Text = Playlists[position].Name;

            if(Playlists[position].SongContained)
                holder.Added.Checked = true;
            else
                holder.Added.Checked = false;


            if (Playlists[position].SyncState == SyncState.True)
            {
                holder.Status.Visibility = ViewStates.Visible;
                holder.Status.SetImageResource(Resource.Drawable.Sync);                    
            }
            else if(Playlists[position].YoutubeID != null)
            {
                holder.Status.Visibility = ViewStates.Visible;
                holder.Status.SetImageResource(Resource.Drawable.PublicIcon);
            }
            else
            {
                holder.Status.Visibility = ViewStates.Gone;
            }

            if (MainActivity.Theme == 1)
            {
                holder.Status.SetColorFilter(Color.White);
                holder.Title.SetTextColor(Color.White);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (viewType == 0)
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.AddToPlaylistItem, parent, false);
                return new AddToPlaylistHolder(itemView, OnClick);
            }
            else
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.smallLoading, parent, false);
                return new UslessHolder(itemView);
            }
        }

        public override int GetItemViewType(int position)
        {
            if (position == Playlists.Count - 1 && Playlists[position].Name == "Loading" && Playlists[position].LocalID == 0 && Playlists[position].YoutubeID == null)
                return 1;
            else
                return 0;
        }

        void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }
    }

    public class AddToPlaylistHolder : RecyclerView.ViewHolder
    {
        public TextView Title;
        public CheckBox Added;
        public ImageView Status;

        public AddToPlaylistHolder(View itemView, Action<int> listener) : base(itemView)
        {
            Title = itemView.FindViewById<TextView>(Resource.Id.title);
            Added = itemView.FindViewById<CheckBox>(Resource.Id.added);
            Status = itemView.FindViewById<ImageView>(Resource.Id.status);

            itemView.Click += (sender, e) => listener(AdapterPosition);
        }
    }
}