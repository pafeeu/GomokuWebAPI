using GomokuWebAPI.Common;
using GomokuWebAPI.Model;
using GomokuWebAPI.Model.Enums;
using Microsoft.EntityFrameworkCore;

namespace GomokuWebAPI.Repositories
{
    public class GameHelper
    {
        private readonly AppDbContext _context;
        public GameHelper(AppDbContext context)
        {
            _context = context;
        }
        public bool IsValidMove(short x, short y)
        {
            return x >= 0 && x < Statics.BOARD_SIZE 
                && y >= 0 && y < Statics.BOARD_SIZE;
        }
        public async Task<bool> IsFreeSlot(long gameId, short x, short y)
        {
            return !await _context.Moves.AnyAsync(g => g.GameId == gameId && g.X == x && g.Y == y);
        }
        /// <returns>The id of the player whose move is now 
        /// or null if it cannot be determined</returns>
        public async Task<long?> WhoseMove(long gameId)
        {
            var game = await _context.Games.Include(x=>x.Moves).SingleOrDefaultAsync(x => x.Id == gameId);
            if (game == null) return null;

            //there was at least one move
            if (game.Moves?.Count>0)
                return game.Players?.FirstOrDefault(p => p.PlayerId != game.Moves.Last().PlayerId)?.PlayerId;

            //it will be first move
            return game.Players?.FirstOrDefault(p => p.Roles.HasFlag(PlayerRole.Invited))?.PlayerId;
        }
        /// <summary>
        /// Checks if anyone has won this game
        /// </summary>
        /// <returns>Id of the winning player or null if no one has won</returns>
        public async Task<long?> CheckWinning(long gameId)
        {
            var game = await _context.Games.Include(x=>x.Moves).Where(x => x.Id == gameId).FirstOrDefaultAsync();
            if (game == null) return null;
            var moves = game.Moves;
            if (moves == null) return null;

            foreach (var move in moves)
            {
                foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                {
                    var coords = new Coords(move.X, move.Y);
                    short counter;
                    for (counter = 1; counter <= 4; counter++)
                    {
                        GetNextCoords(ref coords, dir);
                        var nextMove = moves.Where(m => m.X == coords.x && m.Y == coords.y).FirstOrDefault();
                        if (nextMove is null || nextMove.PlayerId != move.PlayerId)
                            break;
                    }
                    if (counter >= 4)
                        return move.PlayerId;
                }
            }
            return null;
        }
        private enum Direction { RightUp, Right, RightDown, Down }
        private struct Coords { 
            public Coords(short x, short y) 
            { 
                this.x = x; 
                this.y = y; 
            } 
            public short x; 
            public short y; 
        }
        private void GetNextCoords(ref Coords start, Direction direction)
        {
            switch (direction)
            {
                case Direction.RightUp:
                    start.x++; 
                    start.y++;
                    break;
                case Direction.Right:
                    start.x++; 
                    //start.y;
                    break;
                case Direction.RightDown:
                    start.x++; 
                    start.y--;
                    break;
                case Direction.Down:
                    //start.x; 
                    start.y--;
                    break;
                default:break;
            }
        }
    }
}