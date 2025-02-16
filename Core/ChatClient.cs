using App.Contracts;
using Infrastructure.Provider;
using System.Net;

namespace Core
{
    public class ChatClient : ChatBase
    {
        private readonly User _user;
        private readonly IPEndPoint _serverEndpoint;
        private readonly IMessageSource _source;
        private IEnumerable<User> _users = [];
        public ChatClient(string name,IPEndPoint serverEndpoint, IMessageSource source) 
        {
            _serverEndpoint = serverEndpoint;
            _source = source;   
            _user = new User { Name = name };
        
        }

        public override async Task Start()
        {
            var join = new Message { Text = _user.Name, Command = Command.Join };
            await _source.Send(join, _serverEndpoint, CancellationToken);



            Task.Run(Listener);

            while (!CancellationToken.IsCancellationRequested)
            {
                string input = (await Console.In.ReadLineAsync()) ?? string.Empty;
                Message message;
                if (input.Trim().Equals("/exit", StringComparison.CurrentCultureIgnoreCase))
                {
                    message = new()  {SenderId = _user.Id, Command = Command.Exit };
                    await _source.Send(message, _serverEndpoint, CancellationToken);
                    break;
                }
                if(input.Trim().Equals("/users", StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (var user in _users)
                    {
                        Console.WriteLine(user.Name);
                    }
                    continue;
                }
                else
                {
                    message = new() { Text = input, SenderId = _user.Id, Command = Command.None };
                    await _source.Send(message, _serverEndpoint, CancellationToken);
                }
            }
        }
        protected override async Task Listener()
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                try
                {
                    ReceiveResult result = await _source.Receive(CancellationToken);
                    if (result.Message == null)
                    {
                        throw new Exception("Message is null");
                    }
                    if (result.Message.Command == Command.Join)
                    {
                        JoinHandler(result.Message);
                    }

                   else if (result.Message.Command == Command.Users)
                    {
                        UserHandler(result.Message);
                    }

                   else if (result.Message.Command == Command.None)
                    {
                        MessageHandler(result.Message);
                    }
                    else if (result.Message.Command == Command.Confirm)
                    {
                        ConfirmHandler(result.Message);
                    }

                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync(ex.Message);
                }
            }
        }

        private void ConfirmHandler(Message message)
        {
            Console.WriteLine(message.Text);
        }

        private void MessageHandler(Message message)
        {
            Console.WriteLine($"{_users.First(u => u.Id == message.SenderId).Name}: {message.Text}");
        }

        private void UserHandler(Message message)
        {
            _users = message.Users;
        }

        private void JoinHandler(Message message)
        {
            _user.Id = message.RecepentId;
            Console.WriteLine("Join success");
        }

       
    }

}
