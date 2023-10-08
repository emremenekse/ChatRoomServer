using Microsoft.AspNetCore.SignalR;
using SignalRChatServerExample.Data;
using SignalRChatServerExample.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRChatServerExample.Hubs
{
    public class ChatHub : Hub
    {
        public async Task GetNickName(string nickName)
        {
            Client client = new Client
            {
                ConnectionId = Context.ConnectionId,
                NickName = nickName
            };
            var clientCheck = ClientSource.Clients.Any(x => x.NickName == nickName);
            if (!clientCheck)
            {
                ClientSource.Clients.Add(client);
                await Clients.Others.SendAsync("clientJoined", nickName);

                await Clients.All.SendAsync("clients", ClientSource.Clients);
            }
            
        }

        public async Task SendMessageAsync(string message, string clientName)
        {
            clientName = clientName.Trim();
            Client senderClient = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            if(clientName == "Tümü")
            {
                await Clients.Others.SendAsync("receivemessage", message, senderClient.NickName);
            }
            else
            {
                Client client = ClientSource.Clients.FirstOrDefault(c => c.NickName == clientName);
                await Clients.Clients(client.ConnectionId).SendAsync("receivemessage", message, senderClient.NickName);
            }

        }

        public async Task AddGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            Group group = new Group { GroupName = groupName };
            group.Clients.Add(ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId));


            GroupSource.Groups.Add(group);

            await Clients.All.SendAsync("groups", GroupSource.Groups);
        }

        public async Task AddClientToGroup (IEnumerable<string> groupNames)
        {
            Client client = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            foreach (var item in groupNames)
            {
                Group _group = GroupSource.Groups.FirstOrDefault(g => g.GroupName == item);

                var check = _group.Clients != null ? _group.Clients.Any(a => a.ConnectionId == Context.ConnectionId) : false;
                if (!check)
                {
                    _group.Clients.Add(client);
                    await Groups.AddToGroupAsync(Context.ConnectionId, item);
                }
            }
        }

        public async Task GetClientToGroup (string groupName)
        {
            
            Group group = GroupSource.Groups.FirstOrDefault(g => g.GroupName == groupName);

            await Clients.Caller.SendAsync("clients", groupName == "-1" ? ClientSource.Clients : group.Clients  );
        }

        public async Task SendMessageToGroupAsync(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("receiveMessage", message, ClientSource.Clients.FirstOrDefault( c=> c.ConnectionId == Context.ConnectionId).NickName);
        }
    } 
}
