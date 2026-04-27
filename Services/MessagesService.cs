using backEnd.Interfaces;
using backEnd.Models;
using backEnd.Data;
using backEnd.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

namespace backEnd.Services;

public class MessagesService : IMessagesService
{
    private readonly AppDbContext _context;

    public MessagesService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ChatMessage> SendMessageAsync(string senderEmail, ChatMessageDto dto)
    {
        var msg = new ChatMessage
        {
            SenderEmail = senderEmail,
            ReceiverEmail = dto.ReceiverEmail,
            Content = dto.Content,
            Timestamp = DateTime.UtcNow
        };

        _context.ChatMessages.Add(msg);
        await _context.SaveChangesAsync();
        return msg;
    }

    public async Task<IEnumerable<ChatMessage>> GetChatHistoryAsync(string user1Email, string user2Email)
    {
        return await _context.ChatMessages
            .Where(m => (m.SenderEmail == user1Email && m.ReceiverEmail == user2Email) ||
                        (m.SenderEmail == user2Email && m.ReceiverEmail == user1Email))
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetMyContactEmailsAsync(string myEmail)
    {
        // Get all unique emails that the user has either sent to or received from
        var sentTo = await _context.ChatMessages
            .Where(m => m.SenderEmail == myEmail)
            .Select(m => m.ReceiverEmail)
            .Distinct()
            .ToListAsync();

        var receivedFrom = await _context.ChatMessages
            .Where(m => m.ReceiverEmail == myEmail)
            .Select(m => m.SenderEmail)
            .Distinct()
            .ToListAsync();

        return sentTo.Union(receivedFrom).Distinct();
    }
}
