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
            // ✅ 1. Log incoming data
            Console.WriteLine("📩 Received Score Submission:");
            Console.WriteLine($"PlayerId: {data.PlayerId}");
            Console.WriteLine($"PlayerName: {data.PlayerName}");
            Console.WriteLine($"Time: {data.Time}");

            // ✅ 2. Validate race time
            if (data.Time < 1 || data.Time > 600)
                return BadRequest("Invalid race time");

            // ✅ 3. Convert time to milliseconds
            int scoreToSubmit = (int)(data.Time * 1000);

            // ✅ 4. Prepare HTTP client & headers
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", "dev_be8cdc0c9a9943ec80f9e99bfb1be7a5"); // your real dev key
            httpClient.DefaultRequestHeaders.Add("x-lootlocker-game-domain", "dev");

            // ✅ 5. Create the payload
            var payload = new
            {
                member_id = data.PlayerId,
                score = scoreToSubmit,
                leaderboard_id = "30866"
            };

            var json = JsonConvert.SerializeObject(payload);
            Console.WriteLine("📦 Sending to LootLocker: " + json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // ✅ 6. Submit to LootLocker
            var response = await httpClient.PostAsync("https://api.lootlocker.io/game/leaderboards/submit-score", content);
            var lootLockerResponse = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📬 LootLocker Response: {response.StatusCode}");
            Console.WriteLine(lootLockerResponse);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "LootLocker error: " + lootLockerResponse);

            return Ok(new { time = data.Time, score = scoreToSubmit });
        }
    }
}
