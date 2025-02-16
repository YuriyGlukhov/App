using App.Contracts;
using System.Net;

namespace Infrastructure.Provider
{
    public interface IMessageSource
    {
        Task<ReceiveResult> Receive(CancellationToken cancellationToken);
        Task Send(Message message, IPEndPoint endPoint, CancellationToken cancellationToken);
       /* IPEndPoint CreateEndPoint (string address, int port);
        IPEndPoint GetServerEndPoint (string address);*/

    }
}
