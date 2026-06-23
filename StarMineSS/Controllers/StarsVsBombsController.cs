using Microsoft.AspNetCore.Mvc;
using System;
// აქ დაგჭირდება შენი მოდელების using (მაგ: using YourProject.Models;)
namespace StarMineSS.Controllers
{
    [ApiController]
    // დროებით ვტოვებთ ძველ მისამართს, რომ JavaScript-ში fetch-ების შეცვლა არ დაგჭირდეს.
    // თუმცა, მომავალში შეგიძლია აქ [Route("api/starsvsbombs")] დაწერო და JS-შიც შესაბამისად შეცვალო.
    [Route("api/game")]
    public class StarsVsBombsController : ControllerBase
    {
        // 1. ახალი თამაშის დაწყება
        [HttpPost("new")]
        public IActionResult StartNewGame([FromQuery] int rows = 5, [FromQuery] int cols = 5, [FromQuery] int mines = 3)
        {
            // აქ იქნება შენი ლოგიკა, რომელიც აგენერირებს ახალ დაფას
            // var gameState = _gameService.CreateGame(rows, cols, mines);

            // დროებითი Mock პასუხი
            return Ok(new
            {
                id = Guid.NewGuid(),
                rows = rows,
                cols = cols,
                mines = mines,
                gameOver = false,
                board = GenerateHiddenBoard(rows, cols) // დამალული დაფის გენერაცია
            });
        }

        // 2. უჯრის გახსნა
        [HttpPost("{id}/reveal")]
        public IActionResult RevealCell(Guid id, [FromQuery] int r, [FromQuery] int c)
        {
            // აქ იქნება ლოგიკა, რომელიც ამოწმებს ბომბი იყო თუ ვარსკვლავი
            // var updatedState = _gameService.RevealCell(id, r, c);

            return Ok(new
            {
                // განახლებული სტატუსის დაბრუნება
            });
        }

        // დამხმარე მეთოდი (შეგიძლია სერვისში გაიტანო)
        private string[][] GenerateHiddenBoard(int rows, int cols)
        {
            var board = new string[rows][];
            for (int i = 0; i < rows; i++)
            {
                board[i] = new string[cols];
                for (int j = 0; j < cols; j++)
                {
                    board[i][j] = "hidden";
                }
            }
            return board;
        }
    }
}