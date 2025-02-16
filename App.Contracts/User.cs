using Domain;
using System.Net;
using System.Text.Json.Serialization;

namespace App.Contracts
{
    public record User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime LastOnline { get; set; } = DateTime.Now;

        [JsonIgnore]
        public IPEndPoint? EndPoint { get; set; }

        public static User FromDomain(UserEntity entity)
        {
            return new User
            {
                Id = entity.Id,
                Name = entity.Name,
                LastOnline = entity.LastOnline
            };
        }


    }
}
