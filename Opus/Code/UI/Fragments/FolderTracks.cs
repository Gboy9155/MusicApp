﻿using Android.Content;
using Android.Database;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Opus.Adapter;
using Opus.Api;
using Opus.DataStructure;
using Opus.Others;
using Square.Picasso;
using System.Collections.Generic;
using CursorLoader = Android.Support.V4.Content.CursorLoader;

namespace Opus.Fragments
{
    public class FolderTracks : Fragment, LoaderManager.ILoaderCallbacks
    {
        public static FolderTracks instance;
        public string folderName;
        public string path;
        private RecyclerView ListView;
        public BrowseAdapter adapter;
        private TextView EmptyView;
        private string query;


        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            MainActivity.instance.contentRefresh.Refresh += OnRefresh;
            MainActivity.instance.AddFilterListener(Search);

            MainActivity.instance.SupportActionBar.SetDisplayShowTitleEnabled(true);
            MainActivity.instance.FindViewById(Resource.Id.toolbarLogo).Visibility = ViewStates.Gone;
            MainActivity.instance.DisplayFilter();
        }

        public override void OnDestroy()
        {
            MainActivity.instance.contentRefresh.Refresh -= OnRefresh;
            instance = null;
            base.OnDestroy();
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
                LocalManager.PlayInOrder(path, position, "(" + MediaStore.Audio.Media.InterfaceConsts.Title + " LIKE \"%" + query + "%\" OR " + MediaStore.Audio.Media.InterfaceConsts.Artist + " LIKE \"%" + query + "%\")");
            }, (song, position) =>
            {
                More(song, position);
            }, (position) => 
            {
                LocalManager.ShuffleAll(path);
            });
            ListView.SetAdapter(adapter);

            PopulateList();
            return view;
        }

        public static Fragment NewInstance(string path, string folderName)
        {
            instance = new FolderTracks { Arguments = new Bundle() };
            instance.path = path;
            instance.folderName = folderName;
            return instance;
        }

        async void PopulateList()
        {
            if (await MainActivity.instance.GetReadPermission() == false)
            {
                MainActivity.instance.FindViewById(Resource.Id.loading).Visibility = ViewStates.Gone;
                EmptyView.Visibility = ViewStates.Visible;
                EmptyView.Text = GetString(Resource.String.no_permission);
                return;
            }

            LoaderManager.GetInstance(this).InitLoader(0, null, this);
        }

        public Android.Support.V4.Content.Loader OnCreateLoader(int id, Bundle args)
        {
            Uri musicUri = MediaStore.Audio.Media.ExternalContentUri;
            string selection;
            if (query != null)
            {
                selection = MediaStore.Audio.Media.InterfaceConsts.Data + " LIKE \"%" + path + "%\" AND (" + MediaStore.Audio.Media.InterfaceConsts.Title + " LIKE \"%" + query + "%\" OR " + MediaStore.Audio.Media.InterfaceConsts.Artist + " LIKE \"%" + query + "%\")";
                adapter.displayShuffle = false;
            }
            else
            {
                selection = MediaStore.Audio.Media.InterfaceConsts.Data + " LIKE \"%" + path + "%\"";
                adapter.displayShuffle = true;
            }

            return new CursorLoader(Android.App.Application.Context, musicUri, null, selection, null, null);
        }

        public void OnLoadFinished(Android.Support.V4.Content.Loader loader, Object data)
        {
            adapter.SwapCursor((ICursor)data);
        }

        public void OnLoaderReset(Android.Support.V4.Content.Loader loader)
        {
            adapter.SwapCursor(null);
        }

        private void OnRefresh(object sender, System.EventArgs e)
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

        public void More(Song item, int position)
        {
            MainActivity.instance.More(item, () => { LocalManager.PlayInOrder(path, position); });
        }

        public override void OnResume()
        {
            base.OnResume();
            instance = this;
        }

        public override void OnDestroyView()
        {
            MainActivity.instance.SupportActionBar.SetHomeButtonEnabled(false);
            MainActivity.instance.SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            MainActivity.instance.SupportActionBar.SetDisplayShowTitleEnabled(false);
            if (MainActivity.instance.FindViewById(Resource.Id.toolbarLogo) != null)
                MainActivity.instance.FindViewById(Resource.Id.toolbarLogo).Visibility = ViewStates.Visible;
            MainActivity.instance.RemoveFilterListener(Search);
            MainActivity.instance.HideFilter();
            base.OnDestroyView();
        }
    }
}