// ReSharper disable all

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiExample.Models;

namespace WebApiExample.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class RoomsController : ControllerBase

    {

        public record RoomDescription(int Id, string Name, int PlayersCount, int MaxPlayersCount, bool RequiresPassword);

        public record PlayerWrapper(int UserId, string NickName);

        public record RoomWrapper(RoomDescription Description, PlayerWrapper[] Players, DataModels.Track[] songs);

        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<RoomDescription> GetAll() => _context.Rooms.Select(room => new RoomDescription(room.RoomId, room.Name, 1, room.MaxPlayers, room.RequiresPassword)).ToArray();

        [HttpGet("{roomId:int}")]
        public RoomWrapper Get(int roomId)
        {
            var room = _context.Rooms.Where(room => room.RoomId == roomId).Include(room => room.Players).First();

            PlayerWrapper[] players = room.Players.Select(player => new PlayerWrapper(player.PlayerId,player.DisplayName)).ToArray();

            var roomDescriprion =
                new RoomDescription(room.RoomId, room.Name, players.Length, room.MaxPlayers, room.RequiresPassword);


            DataModels.Track[] tracks = _context.Tracks.Where(
                track => _context.PlaylistsToTracks.Where(pt =>
                        pt.PlaylistId == (_context.Rooms.Where(room => room.RoomId == roomId).Select(room =>
                            room.Playlist.PlaylistId).First()
                        ))
                    .Select(pt => pt.TrackId).Contains(track.TrackId)).ToArray();

            return new RoomWrapper(roomDescriprion, players, tracks);

        }
    }
}
