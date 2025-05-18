// RaceGame_ScoreController.cs
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EmailWebApp.Controller
{
    [ApiController]
    [Route("api/score")]
    public class RaceGame_ScoreController : ControllerBase
    {
        private readonly HttpClient _lootClient;

        public RaceGame_ScoreController(IHttpClientFactory httpFactory)
        {
            // Retrieve the named, preconfigured LootLocker client
            _lootClient = httpFactory.CreateClient("LootLocker");
        }

        [HttpPost("submit-score")]
        public async Task<IActionResult> SubmitScore([FromBody] RaceResultDto data)
        {
            if (data.Time < 1 || data.Time > 600)
                return BadRequest("⛔ Invalid race time");
            if (string.IsNullOrWhiteSpace(data.SessionToken))
                return BadRequest("⛔ Missing session token");

            try
            {
                var scoreToSubmit = (int)data.Time;
                var payload = new
                {
                    member_id = data.PlayerId,
                    score = scoreToSubmit
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Create request with session token header
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "game/leaderboards/30866/submit"
                );
                request.Headers.Add("x-session-token", data.SessionToken);
                request.Content = content;

                // Timing the request
                var sw = Stopwatch.StartNew();
                var response = await _lootClient.SendAsync(request);
                sw.Stop();

                Debug.WriteLine($"🕒 LootLocker call took {sw.ElapsedMilliseconds}ms");

                var resultText = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ LootLocker error: {resultText}");
                    return StatusCode((int)response.StatusCode, new
                    {
                        success = false,
                        error = "LootLocker submission failed",
                        detail = resultText
                    });
                }

                return Ok(new
                {
                    success = true,
                    time = data.Time,
                    score = scoreToSubmit
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 Exception in SubmitScore: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Internal server error",
                    detail = ex.Message
                });
            }
        }
    }
}
