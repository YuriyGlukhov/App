using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace App.Contracts.Extensions
{
    public static class MessageExtensions
    {
        public static Message? ToMessage(this byte[] data)
        {
          return  JsonSerializer.Deserialize<Message>(Encoding.UTF8.GetString(data));
        }
        public static byte[] ToBytes(this Message message)
        {
          return  Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        }
    }
}
