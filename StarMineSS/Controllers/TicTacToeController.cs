using Microsoft.AspNetCore.Mvc;
using StarMineSS.Model;
using StarMineSS.Service;

namespace StarMineSS.Controllers
{
    [ApiController]
    [Route("api/tictactoe")]
    public class TicTacToeController(TicTacToeStore store) : ControllerBase
    {
        private static readonly int[][] WinLines =
        {
            new[] { 0, 1, 2 }, new[] { 3, 4, 5 }, new[] { 6, 7, 8 }, // rows
            new[] { 0, 3, 6 }, new[] { 1, 4, 7 }, new[] { 2, 5, 8 }, // columns
            new[] { 0, 4, 8 }, new[] { 2, 4, 6 }                     // diagonals
        };

        // ახალი თამაშის დაწყება
        [HttpPost("new")]
        public IActionResult StartNewGame()
        {
            var game = new TicTacToeGame();
            store.Save(game);
            return Ok(ToDto(game));
        }

        // სვლის გაკეთება
        [HttpPost("{id:guid}/move")]
        public IActionResult MakeMove(Guid id, [FromQuery] int index, [FromQuery] string player)
        {
            var game = store.Get(id);
            if (game is null) return NotFound("Game not found.");
            if (!string.IsNullOrEmpty(game.Winner) || game.IsDraw) return BadRequest("Game already finished.");
            if (index is < 0 or > 8) return BadRequest("Invalid cell index.");
            if (game.Board[index] != "") return BadRequest("Cell already taken.");
            if (!string.Equals(player, game.Turn, StringComparison.OrdinalIgnoreCase)) return BadRequest($"It's {game.Turn}'s turn.");

            game.Board[index] = player;
            ApplyResult(game);

            // Frontend only ever sends X's moves, so the bot answers as O.
            if (string.IsNullOrEmpty(game.Winner) && !game.IsDraw && game.Turn == "O")
            {
                var botMove = PickBotMove(game.Board);
                if (botMove >= 0)
                {
                    game.Board[botMove] = "O";
                    ApplyResult(game);
                }
            }

            store.Save(game);
            return Ok(ToDto(game));
        }

        private static void ApplyResult(TicTacToeGame game)
        {
            var winner = CheckWinner(game.Board);
            if (winner is not null)
            {
                game.Winner = winner;
                return;
            }
            if (Array.TrueForAll(game.Board, cell => cell != ""))
            {
                game.IsDraw = true;
                return;
            }
            game.Turn = game.Turn == "X" ? "O" : "X";
        }

        private static string? CheckWinner(string[] board)
        {
            foreach (var line in WinLines)
            {
                var (a, b, c) = (line[0], line[1], line[2]);
                if (board[a] != "" && board[a] == board[b] && board[b] == board[c])
                    return board[a];
            }
            return null;
        }

        // მარტივი მაგრამ ღირსეული ბოტი: მოიგე თუ შეგიძლია, დაბლოკე თუ საჭიროა, სხვა შემთხვევაში ცენტრი/კუთხე/შემთხვევითი
        private static int PickBotMove(string[] board)
        {
            var winMove = FindLineCompletion(board, "O");
            if (winMove is not null) return winMove.Value;

            var blockMove = FindLineCompletion(board, "X");
            if (blockMove is not null) return blockMove.Value;

            if (board[4] == "") return 4;

            foreach (var corner in new[] { 0, 2, 6, 8 })
                if (board[corner] == "") return corner;

            for (int i = 0; i < board.Length; i++)
                if (board[i] == "") return i;

            return -1;
        }

        private static int? FindLineCompletion(string[] board, string symbol)
        {
            foreach (var line in WinLines)
            {
                int emptyIndex = -1, emptyCount = 0, symbolCount = 0;
                foreach (var i in line)
                {
                    if (board[i] == "") { emptyIndex = i; emptyCount++; }
                    else if (board[i] == symbol) symbolCount++;
                }
                if (emptyCount == 1 && symbolCount == 2) return emptyIndex;
            }
            return null;
        }

        private static object ToDto(TicTacToeGame g) => new
        {
            id = g.Id,
            board = g.Board,
            turn = g.Turn,
            winner = g.IsDraw ? "Draw" : g.Winner,
            isDraw = g.IsDraw
        };
    }
}