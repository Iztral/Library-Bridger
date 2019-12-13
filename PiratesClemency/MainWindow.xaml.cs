using PiratesClemency.Spotify.Classes;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace PiratesClemency
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CloudButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SpotifyButton_Click(object sender, RoutedEventArgs e)
        {
            Spotify.Windows.SpotifyWindow spotifyWindow = new Spotify.Windows.SpotifyWindow();
            spotifyWindow.Show();
            this.Close();
        }
    }
}
