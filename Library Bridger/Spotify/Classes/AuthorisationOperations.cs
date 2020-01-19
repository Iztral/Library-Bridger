using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;

namespace LibraryBridger.Spotify.Classes
{
    public class AuthorisationOperations
    {
        private static SpotifyWebAPI _spotify;

        public void Spotify_Setter(ref SpotifyWebAPI _spotify_main)
        {
            _spotify = _spotify_main;
        }

        public void Authorise(string _clientId)
        {
            ImplicitGrantAuth auth = new ImplicitGrantAuth(_clientId,
                "http://localhost:4002", "http://localhost:4002",
                Scope.UserLibraryModify | Scope.PlaylistModifyPrivate | Scope.PlaylistModifyPublic);
            auth.AuthReceived += (sender, payload) =>
            {
                auth.Stop();
                _spotify.TokenType = payload.TokenType;
                _spotify.AccessToken = payload.AccessToken;
            };
            auth.Start();
            auth.OpenBrowser();
        }
    }
}
