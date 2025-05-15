using EmailWebApp.models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace EmailWebApp.controller
{

    [ApiController]
    [Route("api/score")]
    public class RaceGame_ScoreController : ControllerBase
    {
        [HttpPost("submit-score")]
        public async Task<IActionResult> SubmitScore([FromBody] RaceResultDto data)
        {
            // 🔐 Optional anti-cheat: reject crazy values
            if (data.Time < 1 || data.Time > 600)
                return BadRequest("Invalid race time");

            // Convert float time to int milliseconds (LootLocker needs integer score)
            int scoreToSubmit = (int)(data.Time * 1000); // e.g., 42.6s → 42600

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", "dev_be8cdc0c9a9943ec80f9e99bfb1be7a5");

            var payload = new
            {
                member_id = data.PlayerId,
                score = scoreToSubmit,
                leaderboard_id = "30866"
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.lootlocker.io/game/leaderboards/30866/submit", content);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "LootLocker error");

            return Ok(new { time = data.Time, score = scoreToSubmit });
        }
    }
}
