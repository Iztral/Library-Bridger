using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LibraryBridger
{
    /// <summary>
    /// Interaction logic for ReplaceDialog.xaml
    /// </summary>
    public partial class ReplaceDialog : Window
    {
        public FullTrack returnTrack;
        public ReplaceDialog(SearchItem search_result)
        {
            List<ReplacementTrack> list = new List<ReplacementTrack>();
            InitializeComponent();
            foreach(FullTrack track in search_result.Tracks.Items)
            {
                ReplacementTrack repTrack = new ReplacementTrack
                {
                    ImagePath = track.Album.Images[0].Url,
                    Name = track.Artists[0].Name + " - " + track.Name,
                    Spot_track = track
                };
                list.Add(repTrack);
            }
            switchBox.ItemsSource = list;
        }

        private class ReplacementTrack
        {
            public string ImagePath { get; set; }
            public string Name { get; set; }
            public FullTrack Spot_track { get; set; }
        }

        private void SwitchBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(switchBox.SelectedItem != null)
            {
                returnTrack = ((ReplacementTrack)switchBox.SelectedItem).Spot_track;
            }
            this.DialogResult = true;
        }
    }
}
