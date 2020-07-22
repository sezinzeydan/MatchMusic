using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MatchMusic.Models;
using Microsoft.AspNetCore.Authorization;
using SpotifyAPI.Web;

namespace MatchMusic.Controllers
{
    [Authorize(Policy = "Spotify")]
    public class UsersController : Controller
    {
        private readonly MatchMusicContext _context;
        private readonly User _user;
        private readonly SpotifyClientBuilder _spotifyClientBuilder;
        

        public UsersController(MatchMusicContext context, SpotifyClientBuilder spotifyClient)
        {
            _context = context;
            _spotifyClientBuilder = spotifyClient;
            _user = new User();
        }

       
        
        public IActionResult Home(string id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var user = _context.Users
                .Include(usermatch => usermatch.MatchedUsers)
                    .ThenInclude(match => match.Match)
                .Include(userArtist => userArtist.UsersArtists)
                    .ThenInclude(artist => artist.Artist)
                .Include(userTrack => userTrack.UsersTracks)
                    .ThenInclude(track => track.Track)
                .SingleOrDefault(x => x.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

       

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        
        
        public async Task<IActionResult> Build()
        {
            var spotify = await _spotifyClientBuilder.CreateClient();
            if (spotify == null)
            {
                return RedirectToAction("Index", "Home");
            }

            PrivateUser me = await spotify.UserProfile.Current();
            _user.UserId = me.Id;
            _user.UserName = me.DisplayName;
            if (me.Images.Any())
            {
                _user.UserProfilePicture = me.Images[0].Url;
            }
            else
            {
                _user.UserProfilePicture = "";
            }

            

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == me.Id);
            var userNum = _context.Users.Count() + 1;
            _user.UserCode = GenerateUserCode(me.Id) + userNum.ToString();
            
            if ( user == null)
            {
                PersonalizationTopRequest request = new PersonalizationTopRequest();
                request.Limit = 50;
                
                var tracks = await spotify.Personalization.GetTopTracks(request);
                List<FullTrack>.Enumerator tracksEnumerator = tracks.Items.GetEnumerator();

                

                var trackNumbers = tracks.Items.Count;
                for (int i = 0; i < trackNumbers; i++)
                {
                    tracksEnumerator.MoveNext();
                    
                    if (tracksEnumerator.Current != null)
                    {
                        
                        //creating a new track and userTracks to map tracks and users
                        Track newTrack = new Track();
                        UserTrack userTracks = new UserTrack();
                        
                        //initializing tracks one bu one
                        newTrack.TrackId = tracksEnumerator.Current.Id;
                        newTrack.TrackName = tracksEnumerator.Current.Name;
                        if (tracksEnumerator.Current.Album.Images.Any())
                        {
                            newTrack.TrackAlbumPicture = tracksEnumerator.Current.Album.Images[0].Url;
                        }
                        else
                        {
                            newTrack.TrackAlbumPicture = "";
                        }
                        newTrack.TrackAlbumPicture = tracksEnumerator.Current.Album.Images[0].Url;
                       
                        //adding track and user to UserTracks model initializing userTracks
                        userTracks.Track = newTrack;
                        userTracks.User = _user;
                        
                        //adding userTracks to User model
                        _user.UsersTracks.Add(userTracks);

                        //adding userTracks to Track model
                        newTrack.UsersTracks.Add(userTracks);
                        //check if artist exists in db
                        var trackExists = _context.Tracks.Count(a => a.TrackId == newTrack.TrackId);
                        if (trackExists == 0)
                        {
                            await _context.Tracks.AddAsync(newTrack);
                        }
                        else
                        {
                            _context.Tracks.Update(newTrack);
                        }
                        
                    }
                    
                }

                //now put top artist to database
                
                var artists = await spotify.Personalization.GetTopArtists(request);
                List<FullArtist>.Enumerator artistsEnumerator = artists.Items.GetEnumerator();

                var artistCount = artists.Items.Count;
                for (int i = 0; i < artistCount; i++)
                {
                    artistsEnumerator.MoveNext();

                    if (artistsEnumerator.Current != null)
                    {
                        //creating a new track and userTracks to map tracks and users
                        Artist newArtist = new Artist();
                        UserArtist userArtist = new UserArtist();

                        //initializing tracks one bu one
                        newArtist.ArtistId = artistsEnumerator.Current.Id;
                        newArtist.ArtistName = artistsEnumerator.Current.Name;

                        if (artistsEnumerator.Current.Images.Any())
                        {
                            newArtist.ArtistPicture = artistsEnumerator.Current.Images[0].Url;
                        }
                        else
                        {
                            newArtist.ArtistPicture = "";
                        }
                        //adding track and user to UserTracks model initializing userTracks
                        userArtist.Artist = newArtist;
                        userArtist.User = _user;

                        //adding userTracks to User model
                        _user.UsersArtists.Add(userArtist);

                        //adding userTracks to Track model
                        newArtist.UsersArtists.Add(userArtist);

                        //check if artist exists in db
                        var artistExists = _context.Artists.Count(a => a.ArtistId == newArtist.ArtistId);
                        if (artistExists == 0)
                        {
                            await _context.Artists.AddAsync(newArtist);
                        }
                        else
                        {
                            _context.Artists.Update(newArtist);
                        }

                    }

                }
                
                await _context.Users.AddAsync(_user);
                
                await _context.SaveChangesAsync();
                
                return RedirectToAction(actionName: "Home","Users",new {id = _user.UserId});
            }
            
            

            return RedirectToAction(actionName: "Home", "Users", new { id = _user.UserId });
        }

        public string GenerateUserCode(string userId)
        {
            
            var secChar = userId[1];
            Dictionary<char,string> secCharToNames  = new Dictionary<char, string>()
            {
                {'1', "fancy-"},
                {'2', "violet-"},
                {'3', "dirty-"},
                {'4', "hot-"},
                {'5',"cheeky-"},
                {'6', "juicy-"},
                {'7', "crazy-"},
                {'8', "funky-"},
                {'9', "fun-"},
                {'a', "thirsty-"},
                {'b',"energy-"},
                {'c', "cry-"},
                {'d', "fab-"},
                {'e', "ugly-"},
                {'f', "cute-"},
                {'g', "boring-"},
                {'h', "shiny-"},
                {'i', "princess-"},
                {'j', "prince-"},
                {'k', "sweet-"},
                {'l', "hard-"},
                {'m', "dirty-"},
                {'p', "silly-"},
                {'r', "fat-"},
                {'s', "jolly-"},
                {'t', "orange-"},
                {'u', "sour-"},
                {'v', "witty-"},
                {'y', "yummy-"},
                {'z', "nutty-"}

            };
            var lastNumber = userId[userId.Length - 1];

            Dictionary<char,string> numbersToNames = new Dictionary<char, string>()
            {
                {'0', "pasta"},
                {'1', "peach"},
                {'2',"cinnamon"},
                {'3', "sugar"},
                {'4', "fairy"},
                {'5', "cookie"},
                {'6', "honey"},
                {'7', "chicken"},
                {'8', "mermaid"},
                {'9', "cat"},
                {'a', "vampire"},
                {'b',"turd"},
                {'c', "baby"},
                {'d', "mouse"},
                {'e', "cake"},
                {'f', "bird"},
                {'g', "turtle"},
                {'h', "cheetos"},
                {'i', "tomato"},
                {'j', "potato"},
                {'k', "veggie"},
                {'l', "lemonade"},
                {'m', "martini"},
                {'p', "rat"},
                {'r', "smoothie"},
                {'s', "cloud"},
                {'t', "lipstick"},
                {'u', "butterfly"},
                {'v', "zombie"},
                {'y', "star"},
                {'z', "dinosaur"}
            };
            string userCode = secCharToNames[secChar] + numbersToNames[lastNumber];
            return userCode;
        }
        

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = _context.Users
                .Include(usermatch => usermatch.MatchedUsers)
                .ThenInclude(match => match.Match)
                .SingleOrDefault(x => x.UserId == id);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index","Home");
        }

        public IActionResult NotFound(string id)
        {
            ViewBag.MyString = id;
            var user = _context.Users
                .Include(usermatch => usermatch.MatchedUsers)
                .ThenInclude(match => match.Match)
                .Include(userArtist => userArtist.UsersArtists)
                .ThenInclude(artist => artist.Artist)
                .Include(userTrack => userTrack.UsersTracks)
                .ThenInclude(track => track.Track)
                .SingleOrDefault(x => x.UserId == id);
            return View("Home", user);
        }

        

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
