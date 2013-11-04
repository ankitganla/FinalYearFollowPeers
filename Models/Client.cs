using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace SignalR.Chat.Models
{
    public class Client
    {
        public string Name { get; set; }
        [ScriptIgnore]
        public DateTime LastResponse { get; set; }
    }
}
