using backEnd.Models;
using backEnd.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backEnd.Interfaces;

public interface IMessagesService
{
    Task<ChatMessage> SendMessageAsync(string senderEmail, ChatMessageDto dto);
    Task<IEnumerable<ChatMessage>> GetChatHistoryAsync(string user1Email, string user2Email);
    Task<IEnumerable<string>> GetMyContactEmailsAsync(string myEmail);
}
