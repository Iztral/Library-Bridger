using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;

namespace PiratesClemency.Classes
{
    public class PlaylistOperations
    {
        private static SpotifyWebAPI _spotify;

        public void Spotify_Setter(ref SpotifyWebAPI _spotify_main)
        {
            _spotify = _spotify_main;
        }

        public void CreatePlaylist(string userId, string name, List<FullTrack> songs, bool? isPrivate, bool? isLiked)
        {
            FullPlaylist playlist = _spotify.CreatePlaylist(userId, name, isPrivate.GetValueOrDefault());
            AddTracks(playlist.Id, songs, isLiked.GetValueOrDefault());
        }

        private void AddTracks(string playlistId, List<FullTrack> songs, bool? isLiked)
        {
            List<string> songUri = new List<string>();
            foreach (FullTrack track in songs)
            {
                songUri.Add(track.Uri);
            }
            foreach (List<string> listLimit in SplitList<string>(songUri, 99))
            {
                ErrorResponse response = _spotify.AddPlaylistTracks(playlistId, listLimit);
            }

            //like added songs//
            if (isLiked.GetValueOrDefault())
            {
                List<string> songIds = new List<string>();
                foreach (FullTrack track in songs)
                {
                    songIds.Add(track.Id);
                }
                foreach (List<string> listLimit in SplitList<string>(songIds, 99))
                {
                    ErrorResponse response = _spotify.SaveTracks(listLimit);
                }
            }
        }

        //split big list into smaller list for queries to spotify API(100 max)//
        public static List<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            var list = new List<List<T>>();

            for (int i = 0; i < locations.Count; i += nSize)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }

            return list;
        }
    }
}
