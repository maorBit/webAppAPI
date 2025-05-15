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
            Console.WriteLine("📩 Incoming Score Submission:");
            Console.WriteLine($"PlayerId: {data.PlayerId}, Name: {data.PlayerName}, Time: {data.Time}");

            if (data.Time < 1 || data.Time > 600)
                return BadRequest("Invalid race time");

            int scoreToSubmit = (int)(data.Time * 1000);

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", "dev_be8cdc0c9a9943ec80f9e99bfb1be7a5");
            httpClient.DefaultRequestHeaders.Add("x-lootlocker-game-domain", "dev");

            // ✅ 1. Set the display name (optional but visible on leaderboard UI)
            var namePayload = new { name = data.PlayerName };
            var nameJson = JsonConvert.SerializeObject(namePayload);
            var nameContent = new StringContent(nameJson, Encoding.UTF8, "application/json");

            var setNameResponse = await httpClient.PostAsync(
                $"https://api.lootlocker.io/game/players/{data.PlayerId}/name",
                nameContent
            );

            var nameResult = await setNameResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"👤 SetName response: {setNameResponse.StatusCode} → {nameResult}");

            // ✅ 2. Submit score
            var scorePayload = new
            {
                member_id = data.PlayerId,
                score = scoreToSubmit
            };

            var scoreJson = JsonConvert.SerializeObject(scorePayload);
            var scoreContent = new StringContent(scoreJson, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(
                "https://api.lootlocker.io/game/leaderboards/30866/submit",
                scoreContent
            );

            var resultText = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📬 SubmitScore response: {response.StatusCode} → {resultText}");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "LootLocker error: " + resultText);

            return Ok(new { time = data.Time, score = scoreToSubmit });
        }
    }
}
