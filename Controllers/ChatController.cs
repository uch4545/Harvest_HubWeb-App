using Data;
using HarvestHub.WebApp.Hubs;
using HarvestHub.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HarvestHub.WebApp.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        #region ==================== CONVERSATIONS ====================

        // GET: /Chat/MyConversations
        public async Task<IActionResult> MyConversations()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var conversations = await _context.Conversations
                    .Include(c => c.Buyer)
                    .Include(c => c.Farmer)
                    .Include(c => c.Crop)
                    .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .Where(c => c.BuyerId == user.Id || c.FarmerId == user.Id)
                    .OrderByDescending(c => c.LastMessageAt)
                    .ToListAsync();

                ViewBag.CurrentUserId = user.Id;
                return View(conversations);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading conversations.";
                LogError("MyConversations", ex);
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Chat/Conversation/5
        public async Task<IActionResult> Conversation(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var conversation = await _context.Conversations
                    .Include(c => c.Buyer)
                    .Include(c => c.Farmer)
                    .Include(c => c.Crop)
                    .Include(c => c.Messages.OrderBy(m => m.SentAt))
                    .FirstOrDefaultAsync(c => c.Id == id &&
                        (c.BuyerId == user.Id || c.FarmerId == user.Id));

                if (conversation == null)
                    return NotFound();

                // Mark unread messages as read
                var unreadMessages = conversation.Messages
                    .Where(m => m.SenderId != user.Id && !m.IsRead);
                
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync();

                ViewBag.CurrentUserId = user.Id;
                ViewBag.CurrentUserName = user.FullName ?? user.Email;
                return View(conversation);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading conversation.";
                LogError("Conversation", ex);
                return RedirectToAction("MyConversations");
            }
        }

        // GET: /Chat/StartConversation?receiverId=xyz
        [HttpGet]
        public async Task<IActionResult> StartConversation(string receiverId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                if (string.IsNullOrEmpty(receiverId))
                {
                    TempData["ErrorMessage"] = "Invalid receiver.";
                    return RedirectToAction("MyConversations");
                }

                // Find existing conversation between these two users (regardless of who started it)
                var existingConversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => 
                        (c.BuyerId == user.Id && c.FarmerId == receiverId) ||
                        (c.FarmerId == user.Id && c.BuyerId == receiverId));

                if (existingConversation != null)
                {
                    return RedirectToAction("Conversation", new { id = existingConversation.Id });
                }

                // Create new conversation - determine roles
                var receiver = await _userManager.FindByIdAsync(receiverId);
                if (receiver == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("MyConversations");
                }

                var conversation = new Conversation
                {
                    BuyerId = user.RoleType == "Buyer" ? user.Id : receiverId,
                    FarmerId = user.RoleType == "Farmer" ? user.Id : receiverId,
                    CropId = null, // General conversation, not tied to specific crop
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow
                };

                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();

                return RedirectToAction("Conversation", new { id = conversation.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error starting conversation.";
                LogError("StartConversation[GET]", ex);
                return RedirectToAction("MyConversations");
            }
        }

        // POST: /Chat/StartConversation
        [HttpPost]
        public async Task<IActionResult> StartConversation(string farmerId, int? cropId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                // Check if conversation already exists
                var existingConversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.BuyerId == user.Id && 
                                              c.FarmerId == farmerId &&
                                              c.CropId == cropId);

                if (existingConversation != null)
                {
                    return RedirectToAction("Conversation", new { id = existingConversation.Id });
                }

                // Create new conversation
                var conversation = new Conversation
                {
                    BuyerId = user.Id,
                    FarmerId = farmerId,
                    CropId = cropId,
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow
                };

                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();

                return RedirectToAction("Conversation", new { id = conversation.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error starting conversation.";
                LogError("StartConversation", ex);
                return RedirectToAction("Dashboard", "Buyer");
            }
        }

        #endregion

        #region ==================== MESSAGES API ====================

        // POST: /Chat/SendMessage (AJAX)
        [HttpPost]
        public async Task<IActionResult> SendMessage(int conversationId, string message)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) 
                    return Json(new { success = false, error = "Unauthorized" });

                if (string.IsNullOrWhiteSpace(message))
                    return Json(new { success = false, error = "Message is empty" });

                // Verify user is part of this conversation
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId &&
                        (c.BuyerId == user.Id || c.FarmerId == user.Id));

                if (conversation == null)
                    return Json(new { success = false, error = "Conversation not found" });

                // Save message
                var chatMessage = new ChatMessage
                {
                    ConversationId = conversationId,
                    SenderId = user.Id,
                    SenderName = user.FullName ?? user.Email ?? "User",
                    Message = message,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.ChatMessages.Add(chatMessage);

                // Update last message time
                conversation.LastMessageAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Broadcast via SignalR
                await _hubContext.Clients.Group($"conversation_{conversationId}")
                    .SendAsync("ReceiveMessage", new
                    {
                        id = chatMessage.Id,
                        senderId = chatMessage.SenderId,
                        senderName = chatMessage.SenderName,
                        message = chatMessage.Message,
                        sentAt = chatMessage.SentAt.ToString("yyyy-MM-dd HH:mm:ss")
                    });

                return Json(new { success = true, messageId = chatMessage.Id });
            }
            catch (Exception ex)
            {
                LogError("SendMessage", ex);
                return Json(new { success = false, error = "Error sending message" });
            }
        }

        // GET: /Chat/GetMessages/5 (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetMessages(int conversationId, int? afterId = null)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) 
                    return Json(new { success = false, error = "Unauthorized" });

                // Verify user is part of this conversation
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId &&
                        (c.BuyerId == user.Id || c.FarmerId == user.Id));

                if (conversation == null)
                    return Json(new { success = false, error = "Conversation not found" });

                var query = _context.ChatMessages
                    .Where(m => m.ConversationId == conversationId);

                if (afterId.HasValue)
                {
                    query = query.Where(m => m.Id > afterId.Value);
                }

                var messages = await query
                    .OrderBy(m => m.SentAt)
                    .Select(m => new
                    {
                        id = m.Id,
                        senderId = m.SenderId,
                        senderName = m.SenderName,
                        message = m.Message,
                        sentAt = m.SentAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        isOwn = m.SenderId == user.Id
                    })
                    .ToListAsync();

                return Json(new { success = true, messages });
            }
            catch (Exception ex)
            {
                LogError("GetMessages", ex);
                return Json(new { success = false, error = "Error loading messages" });
            }
        }

        // GET: /Chat/GetUnreadCount 
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) 
                    return Json(new { count = 0 });

                var count = await _context.ChatMessages
                    .Include(m => m.Conversation)
                    .Where(m => (m.Conversation.BuyerId == user.Id || m.Conversation.FarmerId == user.Id) &&
                                m.SenderId != user.Id &&
                                !m.IsRead)
                    .CountAsync();

                return Json(new { count });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }

        #endregion

        #region ==================== ERROR LOGGING ====================

        private void LogError(string actionName, Exception ex)
        {
            try
            {
                var error = new ErrorLog
                {
                    ControllerName = nameof(ChatController),
                    ActionName = actionName,
                    UserId = User?.Identity?.Name,
                    ExceptionMessage = ex.Message,
                    StackTrace = ex.StackTrace
                };
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
            }
            catch { /* Fail silently */ }
        }

        #endregion
    }
}
