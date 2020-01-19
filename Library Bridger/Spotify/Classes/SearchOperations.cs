using NAudio.Wave;
using LibraryBridger.Generic;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace LibraryBridger.Spotify.Classes
{
    public class SearchOperations
    {
        private static SpotifyWebAPI _spotify;

        public enum SearchOrderType
        {
            [Description("Alphabetic")] ALPH, [Description("Reversed Alphabetic")] ALPH_REV, [Description("Date Created")] CREATE,
            [Description("Reversed Date Created")] CREATE_REV, [Description("Date Modified")] MOD, [Description("Reversed Date Modified")] MOD_REV
        }

        //setts the ref from main//
        public void Spotify_Setter(ref SpotifyWebAPI _spotify_main)
        {
            _spotify = _spotify_main;
        }

        #region operations on local

        private void ScanSubdirectory(DirectoryInfo di, ref List<FileSystemInfo> files, int searchDepth)
        {
            files.AddRange(di.GetFileSystemInfos());

            if(searchDepth == 1)
            {
                foreach (DirectoryInfo subdirectory in di.GetDirectories())
                {
                    ScanSubdirectory(subdirectory, ref files, searchDepth);
                }
            }
        }

        //get list of files from chosen folder//
        public List<LocalTrack> GetLocalTrack_List(SearchOrderType order, int searchDepth)
        {
            List<LocalTrack> _Tracks = new List<LocalTrack>();

            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    List<string> filePaths = new List<string>();
                    DirectoryInfo di = new DirectoryInfo(fbd.SelectedPath);

                    List<FileSystemInfo> files = new List<FileSystemInfo>();

                    ScanSubdirectory(di, ref files, searchDepth);


                    switch (order)
                    {
                        case SearchOrderType.ALPH:
                            filePaths = files.OrderBy(f => f.Name).Select(x => x.FullName).ToList();
                            break;
                        case SearchOrderType.ALPH_REV:
                            filePaths = files.OrderByDescending(f => f.Name).Select(x => x.FullName).ToList();
                            break;
                        case SearchOrderType.CREATE:
                            filePaths = files.OrderBy(f => f.CreationTime).Select(x => x.FullName).ToList();
                            break;
                        case SearchOrderType.CREATE_REV:
                            filePaths = files.OrderByDescending(f => f.CreationTime).Select(x => x.FullName).ToList();
                            break;
                        case SearchOrderType.MOD:
                            filePaths = files.OrderBy(f => f.LastWriteTime).Select(x => x.FullName).ToList();
                            break;
                        case SearchOrderType.MOD_REV:
                            filePaths = files.OrderByDescending(f => f.LastWriteTime).Select(x => x.FullName).ToList();
                            break;
                    }

                    foreach (string file in filePaths)
                    {
                        try
                        {
                            var extension = Path.GetExtension(file);
                            if (extension == ".mp3")
                            {
                                _Tracks.Add(GetLocal_Track(file));
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            return _Tracks;
        }

        //gets tags and path from one file//
        private LocalTrack GetLocal_Track(string file)
        {
            TagLib.File file_tags = TagLib.File.Create(file);
            LocalTrack local_ = new LocalTrack
            {
                File_name = Filters.Filter_word(Path.GetFileNameWithoutExtension(file)),
                Author = Filters.Filter_word(file_tags.Tag.FirstPerformer),
                Title = Filters.Filter_word(file_tags.Tag.Title),
                Path = file
            };
            file_tags.Dispose();

            if (local_.Author != null && local_.Title != null)
            {
                local_.TagState = TagState.FULL_TAGS;
            }
            else if (local_.Author == null || local_.Title == null)
            {
                local_.TagState = TagState.MISSING_TAG;
            }
            else if (local_.Author == null && local_.Title == null)
            {
                local_.TagState = TagState.TITLE_ONLY;
            }
            return local_;
        }
        #endregion

        #region operations on spotify
        //get spotify tracks from local list//
        public List<FullTrack> GetSpotifyTrack_List(ref List<LocalTrack> _Tracks, int CopyBehavior, BackgroundWorker bw)
        {
            #region statistics
            int total_count = _Tracks.Count;
            int found_count = 0;
            int time_elapsed = 0; //seconds//
            Stopwatch watch = new Stopwatch();
            
            #endregion

            List<FullTrack> spotify_List = new List<FullTrack>();
            int index = 0;
            if (Directory.Exists("Files Not Found"))
            {
                System.IO.Directory.Delete("Files Not Found", true);
            }

            watch.Start();

            foreach (LocalTrack local_ in _Tracks)
            {
                if (bw.CancellationPending)
                {
                    break;
                }
                else
                {
                    var results = GetSpotifyTrack(local_, 1);
                    if (results.Tracks.Items.Count > 0)
                    {
                        found_count++;
                        var track = results.Tracks.Items[0];
                        spotify_List.Add(track);
                        local_.SpotifyUri = track.Id;
                    }
                    else
                    {
                        System.IO.Directory.CreateDirectory("Files Not Found");
                        local_.SpotifyUri = "not found";
                        if (CopyBehavior == 0)
                        {
                            File.AppendAllText("Files Not Found\\" + "not_found_tracks.txt", 
                                local_.File_name + Environment.NewLine);
                        }
                        else if (CopyBehavior == 1)
                        {

                            var destFile = AppDomain.CurrentDomain.BaseDirectory + 
                                "Files Not Found\\" + Path.GetFileName(local_.Path);
                            File.Copy(local_.Path, destFile);
                        }
                    }
                    index++;
                    bw.ReportProgress(index, spotify_List);
                }
            }

            #region statistics
            watch.Stop();
            time_elapsed = (int)watch.Elapsed.TotalSeconds; 
            found_count = spotify_List.Count;
            File.WriteAllText("statistics.txt", "Total number of songs: " + total_count + Environment.NewLine 
                + "Number of found songs: " + found_count + Environment.NewLine
                + "Time elapsed (in seconds): " + time_elapsed);
            #endregion

            if (Directory.Exists("Files Not Found"))
            {
                Process.Start(AppDomain.CurrentDomain.BaseDirectory);
            }
            


            return spotify_List;
        }

        //searches for track in spotify with specified keywords//
        public SearchItem GetSpotifyTrack(LocalTrack local_, int limit)
        {
            SearchItem search_results = new SearchItem();

            SearchFor(local_, ref search_results, limit);

            //too many requests//
            if (search_results.Error != null)
            {
                if (search_results.Error.Status == 429)
                {
                    Thread.Sleep(1100);
                    ErrorRepeat(local_, ref search_results, limit);
                }
            }

            //check if tags are wrong//
            if (search_results.Tracks.Items.Count == 0 && local_.TagState == TagState.FULL_TAGS)
            {
                local_.TagState = TagState.TITLE_ONLY;
                SearchFor(local_, ref search_results, limit);
                if (search_results.HasError())
                {
                    ErrorRepeat(local_, ref search_results, limit);
                }
            }

            //if everything returned 0 results fry fingerprinting//
            if(search_results.Tracks.Total == 0)
            {
                try
                {
                    var file = TagLib.File.Create(@local_.Path);
                    var duration = (int)file.Properties.Duration.TotalSeconds;
                    file.Dispose();

                    Fingerprinting.DetectionOperations generator = new Fingerprinting.DetectionOperations();
                    var fingerprint = generator.GenerateFingerprint(local_.Path);
                    var fingerTrack = generator.LookUpFingerprint(fingerprint, duration);
                    if (fingerTrack.Error == null && (fingerTrack.Author == null || fingerTrack.Title == null))
                    {
                        local_.Author = fingerTrack.Author;
                        local_.Title = fingerTrack.Title;
                        SearchFor(local_, ref search_results, limit);
                    }
                }
                catch
                {

                }
            }

            return search_results;
        }

        //search for song in spotify, considers tag state of file//
        private static void SearchFor(LocalTrack local_, ref SearchItem search_results, int limit)
        {
            switch (local_.TagState)
            {
                case TagState.FULL_TAGS:
                    search_results = _spotify.SearchItems(local_.Author + "+" + local_.Title, SpotifyAPI.Web.Enums.SearchType.Track, limit);
                    break;
                case TagState.MISSING_TAG:
                    search_results = _spotify.SearchItems(local_.File_name, SpotifyAPI.Web.Enums.SearchType.Track, limit);
                    break;
                case TagState.TITLE_ONLY:
                    search_results = _spotify.SearchItems(local_.File_name, SpotifyAPI.Web.Enums.SearchType.Track, limit);
                    break;
            }
        }

        //repeat last request if encountered error//
        private void ErrorRepeat(LocalTrack local_, ref SearchItem search_results, int limit)
        {
            int retry = 0;
            while (search_results.Error.Status == 429 || search_results.Error.Status == 502)
            {
                retry++;

                SearchFor(local_, ref search_results, limit);

                if (retry == 3 || search_results.Error == null)
                {
                    break;
                }
            }
        }
        #endregion
    }
}
