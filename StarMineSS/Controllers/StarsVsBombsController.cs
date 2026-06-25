using Microsoft.AspNetCore.Mvc;
using System;
using StarMineSS.Model;
using StarMineSS.Service;

namespace StarMineSS.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class StarsVsBombsController : ControllerBase
    {
        private readonly GameStore _store;

        // Dependency Injection-ით შემოგვაქვს GameStore
        public StarsVsBombsController(GameStore store)
        {
            _store = store;
        }

        [HttpPost("new")]
        public IActionResult StartNewGame([FromQuery] int rows = 5, [FromQuery] int cols = 5, [FromQuery] int mines = 3)
        {
            var g = GameState.Create(rows, cols, mines);
            _store.Save(g);
            return Ok(g.ToClient());
        }

        [HttpPost("{id:guid}/reveal")]
        public IActionResult RevealCell(Guid id, [FromQuery] int r, [FromQuery] int c)
        {
            var g = _store.Get(id);
            if (g is null) return NotFound(new { error = "not_found" });

            g.Reveal(r, c);
            _store.Save(g);
            return Ok(g.ToClient());
        }
    }
}