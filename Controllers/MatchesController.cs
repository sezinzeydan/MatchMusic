using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatchMusic.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SpotifyAPI.Web;

namespace MatchMusic.Controllers
{
    public class MatchesController : Controller
    {
        private readonly MatchMusicContext _context;
        private readonly SpotifyClientBuilder _spotifyClientBuilder;
        private User _currentUser;
        private User _secondUser;
        private Match _match;
        public MatchesController(MatchMusicContext context, SpotifyClientBuilder spotifyClient)
        {
            _context = context;
            _spotifyClientBuilder = spotifyClient;
            _currentUser = new User();
            _secondUser = new User();
            _match = new Match();
        }

        public async Task SetCurrentUser()
        {
            var spotifyClient = await _spotifyClientBuilder.CreateClient();

            var user = await spotifyClient.UserProfile.Current();
            _currentUser.UserId = user.Id;
        }
       

        public async Task<IActionResult> Match(string userCode)
        {
            await SetCurrentUser();

            var secondUser = await _context.Users
                .FirstOrDefaultAsync(m => m.UserCode == userCode);
            if (secondUser == null)
            {
                return RedirectToAction("NotFound", "Users", new {id = _currentUser.UserId});
            }
            _secondUser.UserId = secondUser.UserId;

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == _currentUser.UserId);
            
            if (currentUser == null)
            {
                return RedirectToAction("NotFound", "Users", new { id = _currentUser.UserId });
            }

            if (secondUser.UserId == currentUser.UserId)
            {
                return RedirectToAction("NotFound", "Users", new { id = _currentUser.UserId });
            }
           
            ViewBag.CurrUserName = currentUser.UserName;
            ViewBag.CurrUserPic = currentUser.UserProfilePicture;

            ViewBag.SecUserName = secondUser.UserName;
            ViewBag.SecUserPic = secondUser.UserProfilePicture;

            int exists = 0;
            //check if match exists
            var matchExists = _context.MatchedUsers.Where(mu =>
                mu.UserId == _currentUser.UserId).ToList();//.Where(mu => mu.UserId == secondUser.UserId);

            foreach (var item in matchExists)
            {
                var secondMatch = _context.MatchedUsers.Where(s => s.MatchId == item.MatchId)
                    .Where(s => s.UserId == secondUser.UserId).ToList();
                if (secondMatch.Count() != 0)
                {
                    var oldMatch = _context.Matches
                        .Include(usermatch => usermatch.MatchedUsers)
                        .ThenInclude(user => user.User)
                        .Include(artistmatch => artistmatch.MatchedArtists)
                        .ThenInclude(artist => artist.Artist)
                        .Include(trackmatch => trackmatch.MatchedTracks)
                        .ThenInclude(track => track.Track)
                        .SingleOrDefault(m => m.MatchId == secondMatch[0].MatchId);

                    _match.MatchId = oldMatch.MatchId;
                    _match.MatchRate = oldMatch.MatchRate;
                    _match.MatchDate = oldMatch.MatchDate;
                    _match.MatchedUsers = oldMatch.MatchedUsers;
                    _match.MatchedArtists = oldMatch.MatchedArtists;
                    _match.MatchedTracks = oldMatch.MatchedTracks;

                    exists = 1;
                    return View(_match);
                }
            }
//            var matchExists = await _context.MatchedUsers.FirstOrDefaultAsync((d => d.UserId == _currentUser.UserId && d.UserId == _secondUser.UserId) );

           


            

            if (exists == 0)
            {
                DateTime today = DateTime.Today;
                _match.MatchDate = today;
                _match.MatchName = currentUser.UserName + " and " + secondUser.UserName;
                await _context.Matches.AddAsync(_match);
                await _context.SaveChangesAsync();
            }
            
            

            await AddUsersMatches(currentUser, _match);
            await AddUsersMatches(secondUser, _match);

            await AddMatchingTracks(currentUser, secondUser, _match);
            await AddMatchingArtists(currentUser, secondUser, _match);

            await CalculateMatchRate(_match);
            



            return View(_match);
        }
        
        public IActionResult ShowPreviousMatch(string Id)
        {
            var matchId = Int32.Parse(Id);

            //var match = await _context.Matches.FirstOrDefaultAsync(m => m.MatchId == matchId);

            var match = _context.Matches
                .Include(usermatch => usermatch.MatchedUsers)
                .ThenInclude(user => user.User)
                .Include(artistmatch => artistmatch.MatchedArtists)
                .ThenInclude(artist => artist.Artist)
                .Include(trackmatch => trackmatch.MatchedTracks)
                .ThenInclude(track => track.Track)
                .SingleOrDefault(m => m.MatchId == matchId);

            if (match == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var matchedUsers = match.MatchedUsers.OrderBy(u => u.MatchId).ToList();

            if (matchedUsers.Count() == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.CurrUserName = matchedUsers[0].User.UserName;
            ViewBag.CurrUserPic = matchedUsers[0].User.UserProfilePicture;

            ViewBag.SecUserName = matchedUsers[1].User.UserName;
            ViewBag.SecUserPic = matchedUsers[1].User.UserProfilePicture;

            return View("Match", match);
        }

        public async Task CalculateMatchRate(Match match)
        {
            var matchedTrackCount = match.MatchedTracks.Count();
            var matchedArtistCount = match.MatchedArtists.Count();

            match.MatchRate = matchedTrackCount + matchedArtistCount;

            _context.Update(match);
            await _context.SaveChangesAsync();
        }
        
        public async Task AddMatchingTracks(User currentUser, User secondUser, Match match)
        {
            var currentUserTracks = _context.UsersTracks.Where(u => u.UserId == currentUser.UserId).OrderBy(a=> a.TrackId).ToList();

            var secondUserTracks = _context.UsersTracks.Where(s => s.UserId == secondUser.UserId).OrderBy(b => b.TrackId).ToList();

            int current = 0;
            int second = 0;

            while (current < currentUserTracks.Count() && second < secondUserTracks.Count())
            {
                if (String.Equals(currentUserTracks[current].TrackId, secondUserTracks[second].TrackId))
                {
                    //now we got a matching track 
                    //get the Track object from db
                    var matchedTrack = await _context.Tracks.FirstOrDefaultAsync(t => t.TrackId == currentUserTracks[current].TrackId);

                    //create the trackMatch and add objects to it
                    TrackMatch trackMatch = new TrackMatch();

                    trackMatch.Match = match;
                    trackMatch.Track = matchedTrack;

                    match.MatchedTracks.Add(trackMatch);
                    matchedTrack.MatchedTracks.Add(trackMatch);

                    //updating
                    _context.Matches.Update(match);
                    _context.Tracks.Update(matchedTrack);
                    await _context.SaveChangesAsync();

                    current += 1;
                    second += 1;
                }
                if (String.Compare(currentUserTracks[current].TrackId, secondUserTracks[second].TrackId) == -1)
                {
                    current += 1;
                }
                else
                {
                    second += 1;
                }
            }
        }

       
        public async Task AddMatchingArtists(User currentUser, User secondUser, Match match)
        {
            var currentUserArtists = _context.UsersArtists.Where(u => u.UserId == currentUser.UserId).OrderBy(a => a.ArtistId).ToList();

            var secondUserArtists = _context.UsersArtists.Where(s => s.UserId == secondUser.UserId).OrderBy(b => b.ArtistId).ToList();


            int current = 0;
            int second = 0;
            while (current < currentUserArtists.Count() && second < secondUserArtists.Count())
            {
                if (currentUserArtists[current].ArtistId == secondUserArtists[second].ArtistId)
                {
                    //now we got a matching artist 
                    //get the Track object from db
                    var matchedArtist = await _context.Artists.FirstOrDefaultAsync(a => a.ArtistId == currentUserArtists[current].ArtistId);

                    //create the trackMatch and add objects to it
                    ArtistMatch artistMatch = new ArtistMatch
                    {
                        Match = match,
                        Artist = matchedArtist
                    };

                    match.MatchedArtists.Add(artistMatch);
                    matchedArtist.MatchedArtists.Add(artistMatch);

                    //updating
                    _context.Matches.Update(match);
                    _context.Artists.Update(matchedArtist);
                    await _context.SaveChangesAsync();
                    current += 1;
                    second += 1;
                }
                if (String.Compare(currentUserArtists[current].ArtistId, secondUserArtists[second].ArtistId) == -1)
                {
                    current += 1;
                }
                else
                {
                    second += 1;
                }
            }

        }

        public async Task AddUsersMatches(User user, Match match)
        {
            UserMatch userMatches = new UserMatch
            {
                //adding current user to usersMatches
                User = user,
                Match = match
            };

            user.MatchedUsers.Add(userMatches);
            match.MatchedUsers.Add(userMatches);

            _context.Update(match);
            _context.Update(user);
            await _context.SaveChangesAsync();

        }
    }
}
