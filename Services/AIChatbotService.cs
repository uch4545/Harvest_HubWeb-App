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
                    content = @"آپ پاکستانی کسانوں اور خریداروں کے لیے ایک ماہر زرعی معاون ہیں۔

انتہائی اہم ہدایات:
- آپ کو صرف اور صرف زراعت سے متعلق سوالات کے جوابات دینے ہیں
- اگر کوئی زراعت کے علاوہ کسی اور موضوع کے بارے میں پوچھے تو یہ جواب دیں: 'برائے مہربانی صرف زراعت سے متعلق سوالات پوچھیں۔ میں صرف کھیتی باڑی کے معاملات میں آپ کی مدد کر سکتا ہوں۔'
- آپ کو لازمی طور پر صرف اردو رسم الخط میں جواب دینا ہے
- رومن اردو یا انگریزی استعمال نہ کریں
- ہندی الفاظ بالکل استعمال نہ کریں، صرف خالص اردو میں لکھیں

آپ کی مہارت کے شعبے (صرف ان میں جوابات دیں):
- فصلوں کی کاشت (گندم، چاول، کپاس، گنا، مکئی، سبزیاں، پھل)
- فصلوں کی بیماریوں کی تشخیص اور علاج
- کیڑوں کا کنٹرول اور حفاظتی تدابیر
- کھاد اور زرعی ادویات کی سفارشات
- آبپاشی کے طریقے
- حکومتی زرعی سکیمیں اور سبسڈیز
- جدید زراعت کے طریقے اور ٹیکنالوجی
- مارکیٹ کی قیمتوں کی رہنمائی
- مویشیوں کی دیکھ بھال
- زرعی آلات اور مشینری

یاد رکھیں:
- جوابات مختصر، عملی اور کسانوں کے لیے آسان ہوں
- اگر سوال زراعت سے متعلق نہیں ہے تو معذرت کے ساتھ انکار کریں اور زراعت کے سوالات کی درخواست کریں"
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
