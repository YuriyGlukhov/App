using System.Net.Sockets;
using System.Net;
using Infrastructure.Provider;
using Core;
using Infrastructure.Persistence.Contexts;

namespace AppStart
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IPEndPoint serverEndpoint = new(IPAddress.Parse("127.0.0.1"), 12000);
            IMessageSource source;

            if (args.Length == 0)
            {
                source = new MessageSource(new UdpClient(serverEndpoint));

                var chat = new ChatServer(source, new ChatContext());
                await chat.Start();
            }
            else
            {
                source = new MessageSource(new UdpClient());
                var chat = new ChatClient(args[0], serverEndpoint, source);
                await chat.Start();
            }
        }
    }
}
