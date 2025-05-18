using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace EmailWebApp.controller
{
    [ApiController]
    [Route("api/score")]
    public class RaceGame_ScoreController : ControllerBase
    {
        [HttpPost("submit-score")]
        public async Task<IActionResult> SubmitScore([FromBody] RaceResultDto data)
        {
            if (data.Time < 1 || data.Time > 600)
                return BadRequest("Invalid race time");


            if (string.IsNullOrWhiteSpace(data.SessionToken))
                return BadRequest("Missing session token");

            int scoreToSubmit = (int)(data.Time);


            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", "dev_be8cdc0c9a9943ec80f9e99bfb1be7a5");
            httpClient.DefaultRequestHeaders.Add("x-lootlocker-game-domain", "dev");
            httpClient.DefaultRequestHeaders.Add("x-session-token", data.SessionToken);   // ← this line

            var payload = new
            {
                member_id = data.PlayerId,
                score = scoreToSubmit
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.lootlocker.io/game/leaderboards/30866/submit", content);
            var resultText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "LootLocker error: " + resultText);

            return Ok(new { time = data.Time, score = scoreToSubmit });
        }
    }
}
