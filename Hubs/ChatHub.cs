using Data;
using HarvestHub.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HarvestHub.WebApp.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(int conversationId, string message)
        {
            try
            {
                var userId = Context.User?.Identity?.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userId);

                if (user == null || string.IsNullOrEmpty(message))
                    return;

                // Verify user is part of this conversation
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId &&
                        (c.BuyerId == user.Id || c.FarmerId == user.Id));

                if (conversation == null)
                    return;

                // Save message to database
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
                _context.Conversations.Update(conversation);

                await _context.SaveChangesAsync();

                // Broadcast to conversation group
                await Clients.Group($"conversation_{conversationId}")
                    .SendAsync("ReceiveMessage", new
                    {
                        id = chatMessage.Id,
                        senderId = chatMessage.SenderId,
                        senderName = chatMessage.SenderName,
                        message = chatMessage.Message,
                        sentAt = chatMessage.SentAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        isOwn = false // Will be set client-side
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ChatHub SendMessage error: {ex.Message}");
            }
        }

        public async Task JoinConversation(int conversationId)
        {
            try
            {
                var userId = Context.User?.Identity?.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userId);

                if (user == null)
                    return;

                // Verify user is part of this conversation
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId &&
                        (c.BuyerId == user.Id || c.FarmerId == user.Id));

                if (conversation == null)
                    return;

                await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");

                // Mark unread messages as read
                var unreadMessages = await _context.ChatMessages
                    .Where(m => m.ConversationId == conversationId && 
                                m.SenderId != user.Id && 
                                !m.IsRead)
                    .ToListAsync();

                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ChatHub JoinConversation error: {ex.Message}");
            }
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
