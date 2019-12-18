using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AcoustID;
using AcoustID.Audio;
using AcoustID.Web;
using PiratesClemency.Spotify.Classes;

namespace PiratesClemency.Fingerprinting
{
    public class Generator
    {

        public string GenerateFingerprint(string file)
        {
            
            var decoder = new NAudioDecoder(file);
            var context = new ChromaContext();

            context.Start(decoder.SampleRate, decoder.Channels);
            decoder.Decode(context.Consumer, 1000);
            context.Finish();

            return context.GetFingerprint();
        }
        
        public LocalTrack LookUpFingerprint(string fingerprint, int duration)
        {
            if (String.IsNullOrEmpty(AcoustID.Configuration.ClientKey))
            {
                Configuration.ClientKey = "7qnNawP8nH";
            }
            LookupService service = new LookupService();
            LocalTrack track = new LocalTrack();
            var context = TaskScheduler.Default;

            var task = service.GetAsync(fingerprint, duration, new string[] { "recordings", "compress" });
            task.Wait();
            // Error handling:
            var successContinuation = task.ContinueWith(t =>
            {
                foreach (var e in t.Exception.InnerExceptions)
                {
                    track.Error = "Webservice error";
                }
            },
            CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, context);

            // On success:
            var failureContinuation = task.ContinueWith(t =>
            {
                track.Error = "Something went wrong";
                var response = t.Result;

                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    track.Error = "Webservice error";
                    return;
                }

                if (response.Results.Count == 0)
                {
                    track.Error = "No results for given fingerprint.";
                    return;
                }

                foreach (var result in response.Results)
                {
                    if(result.Recordings.Count != 0)
                    {
                        track.Error = null;
                        track.Author = result.Recordings[0].Artists[0].Name;
                        track.Title = result.Recordings[0].Title;
                        track.TagState = TagState.FULL_TAGS;
                    }
                }
            },
            CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously, context);
            Task.WaitAny(successContinuation, failureContinuation);
            return track;
        }


    }
}
