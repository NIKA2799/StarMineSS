using Microsoft.AspNetCore.Mvc;
using System;

namespace StarMineSS.Controllers
{
    [ApiController]
    [Route("api/tictactoe")]
    public class TicTacToeController : ControllerBase
    {
        // ახალი თამაშის დაწყება
        [HttpPost("new")]
        public IActionResult StartNewGame()
        {
            // 9 უჯრიანი დაფა კრესტიკი-ნოლიკისთვის
            return Ok(new
            {
                id = Guid.NewGuid(),
                board = new string[9] { "", "", "", "", "", "", "", "", "" },
                turn = "X", // პირველი იწყებს X
                winner = "",
                isDraw = false
            });
        }

        // სვლის გაკეთება
        [HttpPost("{id}/move")]
        public IActionResult MakeMove(Guid id, [FromQuery] int index, [FromQuery] string player)
        {
            // მოგვიანებით აქ დავამატებთ მოგების კომბინაციების (ჰორიზონტალური, ვერტიკალური, დიაგონალური) შემოწმებას
            return Ok(new
            {
                success = true,
                message = "Move received"
            });
        }
    }
}