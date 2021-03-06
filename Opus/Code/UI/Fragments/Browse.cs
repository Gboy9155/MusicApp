﻿using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Opus.Adapter;
using Opus.Api;
using Opus.DataStructure;
using Opus.Others;
using Square.Picasso;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CursorLoader = Android.Support.V4.Content.CursorLoader;
using Uri = Android.Net.Uri;

namespace Opus.Fragments
{
    public class Browse : Fragment, LoaderManager.ILoaderCallbacks
    {
        public static Browse instance;
        public RecyclerView ListView;
        public BrowseAdapter adapter;
        public bool focused = true;

        private string query = null;
        private TextView EmptyView;

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            MainActivity.instance.contentRefresh.Refresh += OnRefresh;
            MainActivity.instance.AddFilterListener(Search);
            ListView.NestedScrollingEnabled = true;
        }

        public override void OnDestroy()
        {
            MainActivity.instance.contentRefresh.Refresh -= OnRefresh;
            MainActivity.instance.RemoveFilterListener(Search);
            base.OnDestroy();
            instance = null;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.CompleteRecycler, container, false);
            view.FindViewById(Resource.Id.loading).Visibility = ViewStates.Visible;
            EmptyView = view.FindViewById<TextView>(Resource.Id.empty);
            ListView = view.FindViewById<RecyclerView>(Resource.Id.recycler);
            ListView.SetLayoutManager(new LinearLayoutManager(Android.App.Application.Context));
            ListView.SetItemAnimator(new DefaultItemAnimator());
            adapter = new BrowseAdapter((song, position) => 
            {
                song = LocalManager.CompleteItem(song);
                SongManager.Play(song);
            }, (song, position) => 
            {
                MainActivity.instance.More(song);
            }, (position) => 
            {
                LocalManager.ShuffleAll();
            });
            ListView.SetAdapter(adapter);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            PopulateList();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return view;
        }

        public async Task PopulateList()
        {
            if (await MainActivity.instance.GetReadPermission() == false)
            {
                MainActivity.instance.FindViewById(Resource.Id.loading).Visibility = ViewStates.Gone;
                EmptyView.Visibility = ViewStates.Visible;
                EmptyView.Text = GetString(Resource.String.no_permission);
                return;
            }

            LoaderManager.GetInstance(this).InitLoader(0, null, this);

            //if (adapter.ItemCount == 0)
            //{
            //    EmptyView.Visibility = ViewStates.Visible;
            //    EmptyView.Text = MainActivity.instance.Resources.GetString(Resource.String.no_song);
            //}
        }

        public Android.Support.V4.Content.Loader OnCreateLoader(int id, Bundle args)
        {
            Uri musicUri = MediaStore.Audio.Media.ExternalContentUri;
            string selection;
            if (query != null)
            {
                selection = MediaStore.Audio.Media.InterfaceConsts.Title + " LIKE \"%" + query + "%\" OR " + MediaStore.Audio.Media.InterfaceConsts.Artist + " LIKE \"%" + query + "%\"";
                adapter.displayShuffle = false;
            }
            else
            {
                selection = null;
                adapter.displayShuffle = true;
            }

            return new CursorLoader(Android.App.Application.Context, musicUri, null, selection, null, MediaStore.Audio.Media.InterfaceConsts.Title + " ASC");
        }

        public void OnLoadFinished(Android.Support.V4.Content.Loader loader, Java.Lang.Object data)
        {
            adapter.SwapCursor((ICursor)data);
        }

        public void OnLoaderReset(Android.Support.V4.Content.Loader loader)
        {
            adapter.SwapCursor(null);
        }

        public static Fragment NewInstance()
        {
            if (instance == null)
                instance = new Browse { Arguments = new Bundle() };
            return instance;
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            if (!focused)
                return;
            Refresh();
            MainActivity.instance.contentRefresh.Refreshing = false;
        }

        public void Refresh()
        {
            adapter.NotifyDataSetChanged();
        }

        public void Search(object sender, Android.Support.V7.Widget.SearchView.QueryTextChangeEventArgs e)
        {
            if (e.NewText == "")
                query = null;
            else
                query = e.NewText;

            LoaderManager.GetInstance(this).RestartLoader(0, null, this);
        }

        public override void OnViewStateRestored(Bundle savedInstanceState)
        {
            base.OnViewStateRestored(savedInstanceState);
            instance.ListView = View.FindViewById<RecyclerView>(Resource.Id.recycler);
        }
    }
}