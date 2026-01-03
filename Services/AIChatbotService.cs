using System.Text;
using System.Text.Json;

namespace HarvestHub.WebApp.Services
{
    public class AIChatbotService
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;

        public AIChatbotService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _apiKey = configuration["GroqApi:ApiKey"] 
                ?? throw new ArgumentNullException("Groq API Key is missing");
            _model = configuration["GroqApi:Model"] ?? "llama-3.1-8b-instant";
            _baseUrl = configuration["GroqApi:BaseUrl"] ?? "https://api.groq.com/openai/v1";
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<string> GetResponseAsync(string userMessage, List<ChatHistoryItem>? conversationHistory = null)
        {
            try
            {
                Console.WriteLine($"[AI Chatbot] Processing message: {userMessage}");

                // Build messages array
                var messages = new List<object>();

                // System message
                messages.Add(new
                {
                    role = "system",
                    content = @"You are an expert agricultural assistant for Pakistani farmers and buyers.

CRITICAL INSTRUCTION:
- You MUST respond ONLY in URDU script (اردو رسم الخط میں)
- DO NOT use Roman Urdu or English
- Use proper Urdu language with correct grammar

EXPERTISE AREAS:
- فصلوں کی کاشت (گندم، چاول، کپاس، گنا، مکئی، سبزیاں)
- بیماریوں کی تشخیص اور علاج
- کیڑوں کا کنٹرول اور کھاد کی سفارشات
- حکومتی سکیمیں اور زرعی سبسڈیز
- جدید زراعت کے طریقے
- مارکیٹ ریٹ اور قیمتوں کی رہنمائی

Keep responses concise, practical, and easy for farmers to understand.
If asked about non-agricultural topics, politely redirect to agriculture in Urdu."
                });

                // Add conversation history if exists
                if (conversationHistory != null && conversationHistory.Any())
                {
                    foreach (var item in conversationHistory.TakeLast(5))
                    {
                        messages.Add(new
                        {
                            role = item.Role.ToLower() == "user" ? "user" : "assistant",
                            content = item.Message
                        });
                    }
                }

                // Add current user message
                messages.Add(new
                {
                    role = "user",
                    content = userMessage
                });

                // Prepare API request
                var requestBody = new
                {
                    model = _model,
                    messages = messages,
                    temperature = 0.7,
                    max_tokens = 500
                };

                var jsonRequest = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Add authorization header
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                Console.WriteLine($"[AI Chatbot] Sending request to Groq API...");

                // Make API call
                var apiUrl = $"{_baseUrl}/chat/completions";
                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[AI Chatbot] Response status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[AI Chatbot] ERROR Response: {responseBody}");
                    
                    // Try to extract error message
                    try
                    {
                        using var errorDoc = JsonDocument.Parse(responseBody);
                        if (errorDoc.RootElement.TryGetProperty("error", out var errorObj))
                        {
                            if (errorObj.TryGetProperty("message", out var errorMsg))
                            {
                                var errorMessage = errorMsg.GetString();
                                Console.WriteLine($"[AI Chatbot] API Error: {errorMessage}");
                                return $"API Error: {errorMessage}";
                            }
                        }
                    }
                    catch { }
                    
                    return $"API se response lene mein masla (Status: {response.StatusCode})";
                }

                // Parse successful response
                using var jsonDoc = JsonDocument.Parse(responseBody);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var messageObj))
                    {
                        if (messageObj.TryGetProperty("content", out var contentElement))
                        {
                            var responseText = contentElement.GetString();
                            if (!string.IsNullOrWhiteSpace(responseText))
                            {
                                Console.WriteLine($"[AI Chatbot] Success! Response length: {responseText.Length}");
                                return responseText.Trim();
                            }
                        }
                    }
                }

                Console.WriteLine("[AI Chatbot] Could not parse response");
                return "Response parse karne mein masla. Dobara try karein.";
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[AI Chatbot] Network Error: {ex.Message}");
                return "Internet connection ki problem hai. Connection check karein.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AI Chatbot] ERROR: {ex.GetType().Name}");
                Console.WriteLine($"[AI Chatbot] Message: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[AI Chatbot] Inner: {ex.InnerException.Message}");
                }

                return "Technical masla aa gaya hai. Baad mein koshish karein.";
            }
        }
    }

    public class ChatHistoryItem
    {
        public string Role { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
