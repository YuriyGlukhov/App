using App.Contracts;
using Infrastructure.Provider;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace Core
{
    public class ChatServer : ChatBase
    {
        private readonly IMessageSource _source;
        private HashSet<User> _users = [];
        private ChatContext _chatContext;


        public ChatServer(IMessageSource source, ChatContext context)
        {
            _source = source;
            _chatContext = context;
        }

        public override async Task Start()
        {
          await Listener();
        }

        protected override async Task Listener()
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                try
                {
                    ReceiveResult result = await _source.Receive(CancellationToken) ?? throw new Exception("Message is null");

                    switch (result.Message.Command)
                    {
                        case Command.None:
                            await MessageHandler(result);
                            break;
                        case Command.Join: 
                            await JoinHandler(result);
                            break;
                        case Command.Exit:
                            await ExitHandler(result);
                            break;
                        case Command.Users:
                            await UsersListHandler(result);
                            break;
                        case Command.Confirm:
                            break;
                    }

                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync(ex.Message);
                }
            }
        }

        private async Task UsersListHandler(ReceiveResult result)
        {
            foreach (var user in _users)
            {
                var userList = new Message { Text = user.Name, Command = Command.Users };

                await _source.Send(
                userList,
                _users.First(u => u.Id == result.Message.SenderId).EndPoint!,
                CancellationToken);
            }
        }

        private async Task ExitHandler(ReceiveResult result)
        {
            var user = User.FromDomain(await _chatContext.Users.FirstAsync(x => x.Id == result.Message!.SenderId));
            user.LastOnline = DateTime.Now;
            await _chatContext.SaveChangesAsync();
            
            _users.Remove(_users.First(u=>u.Id == result.Message!.SenderId));
        }

        private async Task MessageHandler(ReceiveResult result)
        {
            if (result.Message!.RecepentId <0)
            {
                await SendAllAsync(result.Message);
            }
            else
            {
                await _source.Send(
                result.Message,
                _users.First(u => u.Id == result.Message.SenderId).EndPoint!,
                CancellationToken);

                var recipientEndpoint = _users.FirstOrDefault(u => u.Id == result.Message.SenderId)?.EndPoint;
                if (recipientEndpoint != null)
                {
                    await _source.Send(
               result.Message,
               recipientEndpoint,
               CancellationToken);
                }
            }


            using (var ctx = new ChatContext())
            {
                var sender = await ctx.Users.FirstOrDefaultAsync(x => x.Id == result.Message.SenderId);
                var recipient = result.Message.RecepentId < 0
               ? null
               : await ctx.Users.FirstOrDefaultAsync(x => x.Id == result.Message.RecepentId);

                if (result.Message.RecepentId >= 0 && recipient == null)
                {
                    Console.WriteLine($"Ошибка: Получатель с ID {result.Message.RecepentId} не найден в БД.");
                    return;
                }
                var existMessage = new MessageEntity
                {
                    Text = result.Message.Text!,
                    SenderId = sender.Id,
                    RecepientId = recipient?.Id ?? -1
                };
                ctx.Messages.Add(existMessage);
                await ctx.SaveChangesAsync();
            }
            
        }

        private async Task JoinHandler(ReceiveResult result)
        {
            User? user = _users.FirstOrDefault(u => u.Name == result.Message.Text);
            if (user is null) 
            {
                user = new User { Name = result.Message.Text, EndPoint = result.EndPoint };

                var existingUser = await _chatContext.Users.FirstOrDefaultAsync(u => u.Name == user.Name);
                if (existingUser != null)
                {
                    user.Id = existingUser.Id;
                }
                else
                {
                    var newUser = new UserEntity { Name = user.Name! };
                    _chatContext.Users.Add(newUser);
                    await _chatContext.SaveChangesAsync();
                    user.Id = newUser.Id;
                }
                
                
                _users.Add(user);

            }
            user.EndPoint = result.EndPoint;


            await _source.Send(
                 new Message() { Command = Command.Join, RecepentId = user.Id }
                ,user.EndPoint
                ,CancellationToken);

            await SendAllAsync(new Message() { Command = Command.Confirm, Text = $"{user.Name} joined" });
            await SendAllAsync(new Message() { Command = Command.Users, RecepentId = user.Id, Users = _users });

            var unRead = await _chatContext.Messages.Where(x => x.RecepientId == user.Id).ToListAsync();

            foreach (var message in unRead)
            {
                await _source.Send(
                 Message.FromDomain(message)
                , user.EndPoint
                , CancellationToken);
            }
        }

        private async Task SendAllAsync(Message message)
        {
            foreach (var user in _users)
            {
                await _source.Send(
                message
               , user.EndPoint!
               , CancellationToken);
            }
        }
    }

}
