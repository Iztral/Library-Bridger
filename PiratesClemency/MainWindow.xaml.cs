using PiratesClemency.Classes;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace PiratesClemency
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private static SpotifyWebAPI _spotify = new SpotifyWebAPI();
        readonly SearchOperations search = new SearchOperations();
        readonly PlaylistOperations playlistOps = new PlaylistOperations();
        readonly BackgroundWorker backgroundWorker = new BackgroundWorker();

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Local_track> locallist = (List<Local_track>)local_list.ItemsSource;
            search.GetSpotifyTrack_List( ref locallist, (int)e.Argument, sender as BackgroundWorker);
        }
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            found_list.ItemsSource = null;
            found_list.ItemsSource = (List<FullTrack>)e.UserState;
            var item = ((List<FullTrack>)found_list.ItemsSource)[((List<FullTrack>)found_list.ItemsSource).Count - 1];
            found_list.ScrollIntoView(item);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Search_Button.Click -= CancelSearch_Click;
            Search_Button.Click += SearchButton_Click;
            Search_Button.Content = "Local Search";

            if (found_list.Items != null)
            {
                Add_Button.IsEnabled = true;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            SortOrder_ComboBox.ItemsSource = Enum.GetValues(typeof(SearchOperations.SearchOrderType));
            backgroundWorker.DoWork += BackgroundWorker1_DoWork;
            backgroundWorker.ProgressChanged += Worker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
        }


        //authorize access to user profile//
        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            AuthorisationOperations auth = new AuthorisationOperations();
            auth.Spotify_Setter(ref _spotify);
            auth.Authorise("3dbe7290d1e24bc8a85001c2374b4b08");
            search.Spotify_Setter(ref _spotify);
            playlistOps.Spotify_Setter(ref _spotify);
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            if(_spotify.AccessToken != null)
            {
                List<Local_track> list = search.GetLocalTrack_List((SearchOperations.SearchOrderType)SortOrder_ComboBox.SelectedItem);
                local_list.ItemsSource = list;
                if (list != null)
                {
                    progressBar.Maximum = list.Count;
                    Search_Button.IsEnabled = true;
                }
            }
            else
            {
                MessageBox.Show("First you must authorize the application.");
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Search_Button.Click -= SearchButton_Click;
            Search_Button.Click += CancelSearch_Click;
            Search_Button.Content = "Cancel";
            backgroundWorker.RunWorkerAsync(CopyBehavior_ComboBox.SelectedIndex);
        }

        private void CancelSearch_Click(object sender, RoutedEventArgs e)
        {
            backgroundWorker.CancelAsync();
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!backgroundWorker.IsBusy)
            {
                if (!String.IsNullOrWhiteSpace(playlistName.Text))
                {
                    PrivateProfile user = _spotify.GetPrivateProfile();
                    playlistOps.CreatePlaylist(user.Id, playlistName.Text, (List<FullTrack>)found_list.ItemsSource, privacy_CheckBox.IsChecked, Like_CheckBox.IsChecked);
                }
            }
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!backgroundWorker.IsBusy)
            {
                if (found_list.SelectedItem != null)
                {
                    ((List<FullTrack>)found_list.ItemsSource).Remove((FullTrack)found_list.SelectedItem);
                    var old = found_list.ItemsSource;
                    found_list.ItemsSource = null;
                    found_list.ItemsSource = old;
                }
            }
        }

        private void Replace_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!backgroundWorker.IsBusy){
                string selectedSpotify = ((FullTrack)found_list.SelectedItem).Id;
                int selectedIndex = found_list.SelectedIndex;

                var listlocal_ = (List<Local_track>)local_list.ItemsSource;
                Local_track local_ = listlocal_.Find(x => x.SpotifyUri == selectedSpotify);
                if (local_ != null)
                {
                    var list = search.GetSpotifyTrack(local_, 5);
                    ReplaceDialog dialog = new ReplaceDialog(list);
                    if (dialog.ShowDialog() == true && dialog.returnTrack != null)
                    {
                        var old = (List<FullTrack>)found_list.ItemsSource;
                        found_list.ItemsSource = null;
                        old[selectedIndex] = dialog.returnTrack;
                        found_list.ItemsSource = old;
                    }
                }
            }
        }

        private void Find_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!backgroundWorker.IsBusy)
            {
                if (found_list.SelectedItem != null)
                {
                    string selectedSpotify = ((FullTrack)found_list.SelectedItem).Id;
                    var listlocal_ = (List<Local_track>)local_list.ItemsSource;
                    var local_ = listlocal_.Find(x => x.SpotifyUri == selectedSpotify);
                    local_list.SelectedItem = local_;
                    local_list.ScrollIntoView(local_);
                }
            }
        }
    }
}
