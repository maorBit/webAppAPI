using EmailWebApp.models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace EmailWebApp.controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class RaceGame_ScoreController : ControllerBase
    {
        [HttpPost("submit-score")]
        public async Task<IActionResult> SubmitScore([FromBody] RaceResultDto data)
        {
            // ✅ 1. Validate race time
            if (data.Time < 1 || data.Time > 600)
                return BadRequest("Invalid race time");

            // ✅ 2. Convert time to milliseconds 
            int scoreToSubmit = (int)(data.Time * 1000);

            // ✅ 3. Set headers
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", "dev_be8cdc0c9a9943ec80f9e99bfb1be7a5"); // your dev API key
            httpClient.DefaultRequestHeaders.Add("x-lootlocker-game-domain", "dev"); // 👈 this tells LootLocker you're using dev

            // ✅ 4. Create payload with leaderboard_id in the body
            var payload = new
            {
                member_id = data.PlayerId,
                score = scoreToSubmit,
                leaderboard_id = "30866" // your leaderboard ID
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // ✅ 5. POST to the correct URL (no caps, no ID in the URL)
            var response = await httpClient.PostAsync("https://api.lootlocker.io/game/leaderboards/submit-score", content);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "LootLocker error");

            return Ok(new { time = data.Time, score = scoreToSubmit });
        }
    }
}
