using System.Text.Json;

namespace Core
{
    public abstract class ChatBase
    {
        public CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();
        public CancellationToken CancellationToken => CancellationTokenSource.Token;
        protected abstract Task Listener();
        public abstract Task Start();
    }

}
