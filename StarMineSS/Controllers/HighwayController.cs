using Microsoft.AspNetCore.Mvc;
using StarMineSS.Model;
using StarMineSS.Service;

namespace StarMineSS.Controllers
{
    [ApiController]
    [Route("api/highway")]
    public class HighwayController(HighwayStore store) : ControllerBase
    {
        // House edge baked into the crash-probability curve (fake currency only).
        private const double HouseEdge = 0.04;

        [HttpPost("new")]
        public ActionResult New([FromQuery] decimal bet, [FromQuery] int steps = 8)
        {
            if (bet <= 0) return BadRequest("Bet must be greater than zero.");
            steps = Math.Clamp(steps, 3, 12);

            var ladder = BuildLadder(steps);
            var game = new HighwayGame
            {
                Bet = bet,
                Ladder = ladder,
                CrashStep = PickCrashStep(ladder)
            };

            store.Save(game);
            return Ok(ToDto(game));
        }

        [HttpPost("{id:guid}/advance")]
        public ActionResult Advance(Guid id)
        {
            var game = store.Get(id);
            if (game is null) return NotFound();
            if (game.Status != HighwayStatus.Active) return BadRequest("Game already finished.");

            game.CurrentStep++;

            if (game.CurrentStep >= game.CrashStep)
            {
                game.Status = HighwayStatus.Crashed;
                game.Payout = 0;
            }
            else if (game.CurrentStep >= game.Ladder.Count)
            {
                // Reached the final safe step — auto cash-out at the top multiplier.
                game.Status = HighwayStatus.CashedOut;
                game.Payout = game.Bet * (decimal)game.Ladder[^1];
            }

            store.Save(game);
            return Ok(ToDto(game));
        }

        [HttpPost("{id:guid}/cashout")]
        public ActionResult CashOut(Guid id)
        {
            var game = store.Get(id);
            if (game is null) return NotFound();
            if (game.Status != HighwayStatus.Active) return BadRequest("Game already finished.");
            if (game.CurrentStep == 0) return BadRequest("Advance at least one step before cashing out.");

            game.Status = HighwayStatus.CashedOut;
            game.Payout = game.Bet * (decimal)game.Ladder[game.CurrentStep - 1];

            store.Save(game);
            return Ok(ToDto(game));
        }

        private static object ToDto(HighwayGame g) => new
        {
            id = g.Id,
            bet = g.Bet,
            ladder = g.Ladder,
            currentStep = g.CurrentStep,
            status = g.Status.ToString(),
            payout = g.Payout,
            currentMultiplier = g.CurrentStep is > 0 && g.CurrentStep <= g.Ladder.Count
                ? g.Ladder[g.CurrentStep - 1]
                : (double?)null
        };

        private static List<double> BuildLadder(int steps)
        {
            var ladder = new List<double>();
            for (int k = 1; k <= steps; k++)
            {
                double m = 1 + 0.02 * k + 0.006 * k * k;
                ladder.Add(Math.Round(m, 2));
            }
            return ladder;
        }

        // Draws a crash step whose distribution matches "fair odds minus house edge"
        // implied by each multiplier, so risk grows the further the run goes.
        private static int PickCrashStep(List<double> ladder, double houseEdge = HouseEdge)
        {
            double prevSurvival = 1.0;
            for (int i = 0; i < ladder.Count; i++)
            {
                double survivalAtStep = (1 - houseEdge) / ladder[i];
                double conditional = Math.Clamp(survivalAtStep / prevSurvival, 0, 1);

                if (Random.Shared.NextDouble() > conditional)
                    return i + 1; // crashes attempting this step (1-based)

                prevSurvival = survivalAtStep;
            }
            return ladder.Count + 1; // survives the whole ladder
        }
    }
}