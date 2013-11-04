using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;

namespace SignalR.Chat.Models
{
    [HubName("chatHub")]
    public class ChatHub : Hub
    {
        private readonly int TimeoutInSeconds = 30;
        private readonly Chat _chat;

        public ChatHub() : this(Chat.Instance) { }

        public ChatHub(Chat chat)
        {
            _chat = chat;
        }

        public void Join(string myName)
        {
            if (Chat.Clients.Where(x => x.Name.Equals(myName)).Count().Equals(0))
            {
                Chat.Clients.Add(new Client() { Name = myName, LastResponse = DateTime.Now });
                SendMessageServer(string.Format("{0} entered the chat", myName));
                GetClients();
                Caller.Naam = myName;
            }
            else
                throw new Exception("This login is allready in use");
        }

        public void unjoin(string myName)
        {
            if (Chat.Clients.Where(x => x.Name.Equals(myName)).Count() > 0)
            {
                Client item_rem = Chat.Clients.Where(x => x.Name.Equals(myName)).First();
                Chat.Clients.Remove(item_rem);
                SendMessageServer(string.Format("{0} Left the chat", myName));
                GetClients();
                Caller.Naam = myName;
            }
        }

        private void GetClients()
        {
            _chat.GetClients();
        }

        public void SendMessageServer(string message)
        {
            _chat.SpreadMessage(message);
        }

        public void SendMessage(string message)
        {
            if (Chat.Clients.Where(x => x.Name.Equals(Caller.Naam)).Count() > 0)
                _chat.SpreadMessage(string.Format("({0}) <b>{1}</b>: {2}", DateTime.Now.ToString("HH:mm"), Caller.Naam, message));
        }
    }
}