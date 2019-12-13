using PiratesClemency.Spotify.Classes;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace PiratesClemency.Spotify.Windows
{
    public partial class SpotifyWindow : Window
    {
        private static SpotifyWebAPI _spotify = new SpotifyWebAPI();
        readonly SearchOperations searchOps = new SearchOperations();
        readonly PlaylistOperations playlistOps = new PlaylistOperations();
        readonly BackgroundWorker backgroundWorker = new BackgroundWorker();
        readonly BackupOperations backupOps = new BackupOperations();

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            List<LocalTrack> locallist = (List<LocalTrack>)local_list.ItemsSource;
            searchOps.GetSpotifyTrack_List(ref locallist, (int)e.Argument, sender as BackgroundWorker);
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
            Search_Button.Content = "Spotify Search";

            if (found_list.Items != null)
            {
                Add_Button.IsEnabled = true;
            }
        }

        public SpotifyWindow()
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
            searchOps.Spotify_Setter(ref _spotify);
            playlistOps.Spotify_Setter(ref _spotify);
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            if (_spotify.AccessToken != null)
            {
                List<LocalTrack> list = searchOps.GetLocalTrack_List((SearchOperations.SearchOrderType)SortOrder_ComboBox.SelectedItem);
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
                    var response = playlistOps.CreatePlaylist(user.Id, playlistName.Text, (List<FullTrack>)found_list.ItemsSource, privacy_CheckBox.IsChecked, Like_CheckBox.IsChecked);
                    if (response.Error == null)
                    {
                        MessageBox.Show("Playlist " + playlistName.Text + " was created.");
                    }
                    else
                    {
                        MessageBox.Show("Something went wrong. Error:" + Environment.NewLine + response.Error.Message);
                    }
                }
            }
        }

        #region operations on found tracks
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
            if (!backgroundWorker.IsBusy)
            {
                string selectedSpotify = ((FullTrack)found_list.SelectedItem).Id;
                int selectedIndex = found_list.SelectedIndex;

                var listlocal_ = (List<LocalTrack>)local_list.ItemsSource;
                LocalTrack local_ = listlocal_.Find(x => x.SpotifyUri == selectedSpotify);
                if (local_ != null)
                {
                    var list = searchOps.GetSpotifyTrack(local_, 5);
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
                    var listlocal_ = (List<LocalTrack>)local_list.ItemsSource;
                    var local_ = listlocal_.Find(x => x.SpotifyUri == selectedSpotify);
                    local_list.SelectedItem = local_;
                    local_list.ScrollIntoView(local_);
                }
            }
        }
        #endregion

        #region backup
        private void SaveBackup_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(local_list.Items.IsEmpty || found_list.Items.IsEmpty))
            {
                backupOps.WriteToXML((List<LocalTrack>)local_list.ItemsSource, (List<FullTrack>)found_list.ItemsSource);
            }
            else
            {
                MessageBox.Show("One of lists is empty.");
            }
        }

        private void LoadBackup_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_spotify.AccessToken != null)
            {
                if (File.Exists("Backup\\list_local.xml") && File.Exists("Backup\\list_spotify.xml"))
                {
                    List<LocalTrack> listLocal_ = new List<LocalTrack>();
                    List<string> songIds = new List<string>();
                    List<FullTrack> listSpotify_ = new List<FullTrack>();

                    backupOps.ReadFromXML(ref listLocal_, ref songIds);
                    foreach (string songId in songIds)
                    {
                        listSpotify_.Add(_spotify.GetTrack(songId));
                    }

                    if (!(listLocal_.Count == 0 || listSpotify_.Count == 0))
                    {
                        local_list.ItemsSource = listLocal_;
                        found_list.ItemsSource = listSpotify_;
                        Search_Button.IsEnabled = true;
                        Add_Button.IsEnabled = true;
                    }
                    else
                    {
                        MessageBox.Show("Backups corrupted.");
                    }
                }
                else
                {
                    MessageBox.Show("No backup found.");
                }
            }
            else
            {
                MessageBox.Show("Application is unathorized");
            }
        }
        #endregion
    }
}