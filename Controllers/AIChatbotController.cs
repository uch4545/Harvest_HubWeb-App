using HarvestHub.WebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace HarvestHub.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIChatbotController : ControllerBase
    {
        private readonly AIChatbotService _chatbotService;

        public AIChatbotController(AIChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            try
            {
                Console.WriteLine("=== AI Chatbot API Called ===");
                Console.WriteLine($"Message: {request.Message}");
                Console.WriteLine($"History count: {request.ConversationHistory?.Count ?? 0}");

                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    Console.WriteLine("Error: Empty message");
                    return BadRequest(new { error = "Message cannot be empty" });
                }

                Console.WriteLine("Calling chatbot service...");
                var response = await _chatbotService.GetResponseAsync(
                    request.Message, 
                    request.ConversationHistory
                );

                Console.WriteLine($"Response received: {(response != null && response.Length > 50 ? response.Substring(0, 50) + "..." : response)}");

                return Ok(new { 
                    success = true, 
                    response = response,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Controller Error: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                return StatusCode(500, new { 
                    success = false, 
                    error = "Maaf kijiye, kuch galat ho gaya. Baad mein koshish karein." 
                });
            }
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", service = "AI Chatbot" });
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestAPI()
        {
            try
            {
                Console.WriteLine("=== Testing AI API ===");
                
                // Test 1: Simple message
                Console.WriteLine("Test 1: Sending simple test message...");
                var testResponse = await _chatbotService.GetResponseAsync("Hello");
                Console.WriteLine($"Test 1 Response: {testResponse}");
                
                // Test 2: Agriculture question
                Console.WriteLine("Test 2: Sending agriculture question...");
                var agriResponse = await _chatbotService.GetResponseAsync("Wheat ki kashtakari kab karni chahiye?");
                Console.WriteLine($"Test 2 Response: {agriResponse}");
                
                return Ok(new { 
                    success = true, 
                    message = "API tests completed - check console for details",
                    test1 = testResponse,
                    test2 = agriResponse,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test Error: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                
                return Ok(new { 
                    success = false, 
                    error = ex.Message,
                    type = ex.GetType().Name,
                    stack = ex.StackTrace?.Split('\n').Take(5)
                });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = "";
        public List<ChatHistoryItem>? ConversationHistory { get; set; }
    }
}
