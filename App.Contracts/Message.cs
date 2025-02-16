using System.Data;
using Domain;

namespace App.Contracts
{
    public class Message
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public int SenderId { get; set; }
        public int RecepentId { get; set; } = -1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Command Command { get; set; } = Command.None;
        public IEnumerable<User> Users { get; set; } = [];

        public static Message FromDomain(MessageEntity entity)
        {
            return new Message
            {
                Id = entity.Id,
                SenderId = entity.SenderId,
                RecepentId = entity.RecepientId,
                CreatedAt = entity.CreatedAt,

            };
        }
    }
}
